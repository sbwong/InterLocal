import { RootState, store } from "../../app/store";
import reducer, {
	ProfileState,
	initialState,
	loginSuccess,
	logoutSuccess,
	selectIsAuth,
	selectProfile,
	selectUserID,
	setProfileState,
} from "../profileSlice";

import { AxiosResponse } from 'axios';
import { PostState } from "../postSlice";
import expect from "expect";

const appState = store.getState();

const mockProfileOne: ProfileState = {
	isAuth: false,
	isLoaded: true,
	username: "abc123",
	user_id: -1,
	firstName: "John",
	lastName: "Doe",
	college: "Rice",
	email: "jd123@rice.edu",
	phone: "111-222-3333",
	country: "U.S.",
	pictureUrl: "/broken-image.jpg",
	year: "2021",
	isEmailPublic: true,
	isPhonePublic: true,
	isYearPublic: true,
	isCountryPublic: true,
	isResidentialCollegePublic: true,
	password : "", 
	confirmPassword : "", 
	isAdmin: false, 
	totalUpvotes: 0,
};

const mockProfileTwo: Partial<ProfileState>  = {
	isAuth: true,
	isLoaded: true,
	username: "cba321",
	user_id: -1,
	firstName: "Jane",
	lastName: "Doe",
	college: "Brown",
	email: "jd321@rice.edu",
	phone: "333-222-1111",
	country: "M.X.",
	pictureUrl: "/broken-image.jpg",
	year: "2022",
	isEmailPublic: true,
	isPhonePublic: true,
	isYearPublic: true,
	isCountryPublic: true,
	isResidentialCollegePublic: true,
	password : "", 
	confirmPassword : "", 
	isAdmin: false, 
	totalUpvotes: 0,
};

const mockNewProfile: Partial<ProfileState>  = {
	isAuth: true,
	isLoaded: true,
	username: "new123",
	user_id: -1,
	firstName: "New",
	lastName: "ProfileState",
	college: "Brown",
	email: "np@rice.edu",
	phone: "333-222-1111",
	country: "M.X.",
	pictureUrl: "/broken-image.jpg",
	year: "2022",
	isEmailPublic: true,
	isPhonePublic: true,
	isYearPublic: true,
	isCountryPublic: true,
	isResidentialCollegePublic: true,
	bio: "Hello World",
};

// Our mocked response
const axiosResponse: AxiosResponse = {
	data: {
			user_id: "123", 
			status: "user",
		},
	status: 200,
	statusText: 'OK',
	config: {},
	headers: {},
  };

const mockPosts: PostState = {
	posts: [],
	topPosts: [],
	topTags: []
};

// axios mocked
export default {
	// Typescript requires a 'default'
	default: {
	  post: jest.fn().mockImplementation(() => Promise.resolve(axiosResponse)),
	},
	post: jest.fn(() => Promise.resolve(axiosResponse)),
};

describe("profile slice", () => {
	describe("reducer, actions, selectors", () => {
		it("should return the initial state", () => {
			// Arrange
			const nextState = initialState;

			// Act
			const dummyAction: any = {};
			const result = reducer(undefined, dummyAction);

			// Assert
			expect(result).toEqual(nextState);

			// Test out selector too
			const dummyRootState: RootState = {
				...appState,
			    posts: mockPosts,
			    profile: result
			}
			expect(selectProfile(dummyRootState)).toEqual(result);
		});

		it("should set auth to false on logout", () => {
			// mockProfileTwo has its auth field set
			const data = mockProfileTwo;

			// Act
			const result = reducer(data, logoutSuccess());

			// The new state should have its auth field set to false
			expect(result.isAuth).toEqual(false);

			//Verify the new state using a selector
			const dummyRootState: RootState = {
				...appState,
			    posts: mockPosts,
			    profile: result
			}
			expect(selectIsAuth(dummyRootState)).toEqual(false);
		});

		// Test create profile
		it("should set profile state correctly", () => {
			const newState = setProfileState(mockNewProfile);
			expect(newState.payload.firstName).toEqual("New");
			expect(newState.payload.lastName).toEqual("ProfileState");
			
			const revertedState = setProfileState(mockProfileOne);
			expect(revertedState.payload.username).toEqual("abc123");
			expect(revertedState.payload.isCountryPublic).toEqual(true);
		})

		it('should set auth to true and update username on login', () => {
			// mockProfileTwo has its auth field set to false
			const data = mockProfileOne;

			// Act
			const result = reducer(data, loginSuccess(axiosResponse));

			// The new state should have its auth field set to false
			expect(result.user_id).toEqual("123");
			expect(result.isAuth).toEqual(true);

			// Verify the new state using a selector
			const dummyRootState: RootState = {
				...appState,
				posts: mockPosts,
				profile: result
			}
			expect(selectIsAuth(dummyRootState)).toEqual(true);
			expect(selectUserID(dummyRootState)).toEqual("123");
		});
	});
});
