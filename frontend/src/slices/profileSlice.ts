import { AppThunk, RootState } from "../app/store";
import { PayloadAction, createSlice } from "@reduxjs/toolkit";
import {
	makeAPIGetRequest,
	makeAPIPostRequest,
	makeAPIPutRequest,
} from "../adapters/api";

import { AxiosResponse } from "axios";
import { Color } from "@material-ui/lab/Alert";

// ******************************* Interfaces *******************************

// This corresponds to the interface for the application wide state. A ProfileState
// contains basic information about the currently logged in user.
export interface ProfileState {
	isAuth: boolean;
	isLoaded: boolean;
	username: string;
	user_id: number;
	firstName: string;
	lastName: string;
	college: string;
	email: string;
	phone: string;
	country: string;
	pictureUrl: string;
	year: string;
	isEmailPublic: boolean;
	isPhonePublic: boolean;
	isYearPublic: boolean;
	password: string;
	confirmPassword: string;
	isResidentialCollegePublic: boolean;
	isCountryPublic: boolean;
	isAdmin: boolean;
	totalUpvotes: number;
	bio: string;
}

export interface UpdatePasswordPackage {
	currentPassword: string;
	newPassword: string;
	confirmNewPassword: string;
}

// When we initially load into the application, this will be the current state
// of the user's profile. This should be updated once the user logs in. In case
// of network failure, this initialState will be used.
export const initialState: ProfileState = {
	isAuth: false,
	isLoaded: false,
	username: "",
	user_id: -1,
	firstName: "",
	lastName: "",
	college: "",
	email: "",
	phone: "",
	country: "",
	pictureUrl: "/broken-image.jpg",
	year: "",
	isEmailPublic: false,
	isPhonePublic: false,
	isYearPublic: true,
	isCountryPublic: true,
	password: "",
	confirmPassword: "",
	isResidentialCollegePublic: true,
	isAdmin: false,
	totalUpvotes: 0,
	bio: "",
};

// ********************** Action Creators & Reducers **********************

export const profileSlice = createSlice({
	// Name of the this slice
	name: "profile",

	// The initial state as detailed above
	initialState,

	// The reducers which accept dispatched actions
	reducers: {
		// Payload: A ProfileState containing all user information that
		//          will be stored in the application store. Assumes
		//          the user information was received from the backend.
		setProfileState: (state, action: PayloadAction<ProfileState>) => {
			// Payload no longer includes a user_id, so we need to preserve that
			// field from the login response. This will change when migrating
			// to /v2/login, however.
			const userId = state.user_id;
			const isAdmin = state.isAdmin;
			const newState = action.payload;
			newState.user_id = userId;
			newState.isAdmin = isAdmin;
			return newState;
		},

		// Payload: A string corresponding to the username that will be set
		//          in the current ProfileState
		setUsername: (state, action: PayloadAction<string>) => {
			// Call to set username on successful request, will be changed when username and
			// user_id become synonymous post FRTP
			state.username = action.payload;
		},

		// Payload: An AxiosResponse object containing the user_id of
		//          the user who successfully logged in
		loginSuccess: (state, action: PayloadAction<AxiosResponse>) => {
			// At this point, the user has logged in and we know their
			// user_id.
			state.user_id = action.payload.data.user_id;
			state.isAdmin = action.payload.data.status === "admin";
			state.isAuth = true;
			state.isLoaded = false;
		},

		logoutSuccess: (state) => {
			// The user is no longer logged in and the
			// the current ProfileState no longer refers to
			// the user's loaded profile.
			return initialState;
		},
	},
});

// *************************** Actions ***************************

export const {
	setProfileState,
	setUsername,
	loginSuccess,
	logoutSuccess,
} = profileSlice.actions;

// *************************** Selectors ***************************

export const selectFirstName = (state: RootState) => state.profile.firstName;
export const selectIsAuth = (state: RootState) => state.profile.isAuth;
export const selectIsLoaded = (state: RootState) => state.profile.isLoaded;
export const selectName = (state: RootState) =>
	state.profile.firstName + " " + state.profile.lastName;
export const selectProfile = (state: RootState) => state.profile;
export const selectUsername = (state: RootState) => state.profile.username;
export const selectUserID = (state: RootState) => state.profile.user_id;
export const selectIsAdmin = (state: RootState) => state.profile.isAdmin;
export const selectInitials = (state: RootState) => {
	return getInitials(state.profile);
};
// ******************************* Login process *******************************

// Async call to login
export const login = (
	username: string,
	password: string,
	callback: (message: string, severity: Color) => void,
	authErrorHandler: (e: any) => void
): AppThunk => async (dispatch) => {
	try {
		let response = await makeAPIPostRequest(
			"/login",
			{
				username: username,
				password: password,
			},
			"/v2"
		);
		// Fetch profile and other calls currently use userId, will switch to username after FRTP
		console.log("login success");
		console.log(response);

		// Mark user as authenticated
		dispatch(loginSuccess(response));

		// Now actually load the user's profile.
		dispatch(loadProfile(response.data.user_id));

		// Call to set username on successful request, will be changed when username and user_id become synonymous post FRTP
		dispatch(setUsername(username));
		callback("Login success.", "success");
	} catch (e) {
		callback("Login failed.", "error");
		authErrorHandler(e);
		dispatch(catchHandler(e));
	}
};

// Async call that makes call to api and dispatches action to reducer along with
// results as payload
export const loadProfile = (user_id: number): AppThunk => async (dispatch) => {
	try {
		console.log("loading profile", user_id);
		const loadedProfile = await fetchProfile(user_id, (e) =>
			dispatch(catchHandler(e))
		);
		loadedProfile
			? dispatch(setProfileState(loadedProfile))
			: console.log("Loaded profile is undefined");
	} catch (e) {
		dispatch(catchHandler(e));
	}
};

// ******************************* Logout process *******************************

// Async call to logout
export const logout = (
	callback: (message: string, severity: Color) => void
): AppThunk => async (dispatch) => {
	console.log("LOGOUT CALLED");
	try {
		let response = await makeAPIPostRequest("/logout", {});
		console.log(response.data);
		dispatch(logoutSuccess());
		callback("Successfully logout.", "success");
	} catch (e) {
		callback("logout failed.", "error");
		dispatch(catchHandler(e));
	}
};

export function convertProfileResponse(
	profileResponse: AxiosResponse<any>,
	userPrefsResponse: AxiosResponse<any>,
	userID: number
): ProfileState {
	const profile: any = profileResponse.data.res;
	const userPrefs: any = userPrefsResponse.data.res;
	console.log("Profile slice", profile);
	const fetchedProfile: ProfileState = {
		isLoaded: true,
		isAuth: true,
		user_id: userID,
		username: profile.username,
		firstName: profile.first_name,
		lastName: profile.last_name,
		college: profile.college,
		email: profile.email,
		phone: profile.phone_number,
		country: profile.country,
		year: profile.year,
		pictureUrl: profile.pfp_url,
		isEmailPublic: userPrefs.is_email_public,
		isPhonePublic: userPrefs.is_phone_public,
		isCountryPublic: userPrefs.is_country_public,
		isResidentialCollegePublic: userPrefs.is_residential_college_public,
		isYearPublic: userPrefs.is_year_public,
		password: "",
		confirmPassword: "",
		isAdmin: profile.is_admin,
		totalUpvotes: profile.total_upvotes,
		bio: profile.bio,
	};
	return fetchedProfile;
}

// ******************************* View Profile process *******************************

export async function fetchProfile(
	user_id: number,
	onErrorHandler: (e: any) => void
): Promise<ProfileState | undefined> {
	try {
		// TODO: Switch to username after FRTP
		let profile = await makeAPIGetRequest("/user/" + user_id);
		// For some reason, requesting a bad user still returns status 200 but
		// with an empty object, so I am checking that the profile even has
		// a username associated with it.
		if (profile.data.res.username === undefined) {
			return undefined;
		}
		let userPrefs = await makeAPIGetRequest("/user/prefs/" + user_id);
		console.log(profile.data.res);
		console.log(userPrefs.data);
		return convertProfileResponse(profile, userPrefs, user_id);
	} catch (e) {
		onErrorHandler(e);
		console.log(e);
	}
	return undefined;
}

export function getInitials(profile: ProfileState): string {
	var initials: string = "";
	if (profile.firstName.length > 0) {
		initials = initials.concat(profile.firstName.charAt(0));
	}
	if (profile.lastName.length > 0) {
		initials = initials.concat(profile.lastName.charAt(0));
	}
	return initials.toUpperCase();
}

// ****************************** Update Profile process ******************************

export const updateProfile = (
	profile: Partial<ProfileState>,
	postUpdateHandler: () => void
): AppThunk => async (dispatch) => {
	try {
		// TODO: Switch to username after FRTP
		let response = await makeAPIPutRequest("/user/" + profile.user_id, {
			user_id: profile.user_id,
			college: profile.college,
			username: profile.username,
			email: profile.email,
			phone_number: profile.phone,
			country: profile.country,
			year: profile.year,
			bio: profile.bio,
		});

		let userPrefsResponse = await makeAPIPutRequest(
			"/user/prefs/" + profile.user_id,
			{
				is_email_public: profile.isEmailPublic,
				is_phone_public: profile.isPhonePublic,
				is_year_public: profile.isYearPublic,
				is_residential_college_public:
					profile.isResidentialCollegePublic,
				is_country_public: profile.isCountryPublic,
			}
		);

		console.log(response.data);
		console.log(userPrefsResponse.data);

		const updatedProfile: ProfileState = {
			isAuth: true,
			isLoaded: true,
			username: profile.username || "Unknown username",
			user_id: profile.user_id || -1,
			firstName: profile.firstName || "Unknown first name",
			lastName: profile.lastName || "Unknown last name",
			college: profile.college || "Unknown college",
			email: profile.email || "Unknown email",
			phone: profile.phone || "Unknown phone number",
			country: profile.country || "Unknown country",
			pictureUrl: "/broken.jpg",
			year: profile.year || "Unknown year",
			isEmailPublic: profile.isEmailPublic || false,
			isPhonePublic: profile.isPhonePublic || false,
			isYearPublic: profile.isYearPublic || true,
			isCountryPublic: profile.isCountryPublic || true,
			password: "",
			confirmPassword: "",
			isResidentialCollegePublic:
				profile.isResidentialCollegePublic || true,
			isAdmin: false, //to make the compiler happy
			totalUpvotes: profile.totalUpvotes || 0,
			bio: profile.bio || "",
		};

		dispatch(setProfileState(updatedProfile));
		postUpdateHandler();
	} catch (e) {
		dispatch(
			catchHandler(e, () =>
				alert("Profile was unable to be updated. Try again")
			)
		);
	}
};

export const updatePassword = (
	data: Partial<UpdatePasswordPackage>,
	successCallback: () => void
): AppThunk => async (dispatch) => {
	const { currentPassword, newPassword } = data;
	try {
		let updatePasswordResponse = await makeAPIPutRequest("/password", {
			currentPassword,
			newPassword,
		});
		console.log(updatePasswordResponse.data);
		successCallback();
	} catch (e) {
		console.log(e);
		alert(
			"Could not update. Please check your current password and try again"
		);
	}
};

// ****************************** Sign Up process ******************************

export const newProfile = (
	profile: Partial<ProfileState>,
	callback: (message: string, severity: Color) => void
): AppThunk => async (dispatch) => {
	// TODO: Implement a check for duplicate usernames and find a way
	// to redirect back to homepage after a successful signup.
	try {
		let res = await makeAPIPostRequest("/register", {
			username: profile.username,
			password: profile.password,
			first_name: profile.firstName,
			last_name: profile.lastName,
			college: profile.college,
			email: profile.email,
			phone_number: profile.phone,
			country: profile.country,
			year: profile.year,
		});

		if (res.status === 200) {
			callback("Successfully created user.", "success");
			dispatch(
				login(
					profile.username!,
					profile.password!,
					() => {},
					(e) => callback("Failed to log in.", "error")
				)
			);
		}
	} catch (e) {
		console.log(
			"Status: " +
				e.response.status +
				": Failed to create user " +
				profile.username
		);
		callback("Failed to create user.", "error");
		dispatch(catchHandler(e));
	}
};

const handle401 = (): AppThunk => async (dispatch) => {
	console.log("Handling 401...");
	try {
		dispatch(logoutSuccess());
	} catch (e) {
		console.log(e);
	}
};

export const catchHandler = (
	e: any,
	extraHandling: () => void = () => {}
): AppThunk => (dispatch) => {
	console.log(e.response);
	if (e.response !== undefined && e.response.status === 401) {
		dispatch(handle401());
	} else {
		extraHandling();
		console.log(e);
	}
};

export default profileSlice.reducer;
