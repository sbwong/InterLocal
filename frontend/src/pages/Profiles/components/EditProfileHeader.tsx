import { COLLEGES, COUNTRIES, YEARS } from "../../../constants/Constants";
import { IconButton, Menu, Typography } from "@material-ui/core";
import {
	MuiThemeProvider,
	Theme,
	createMuiTheme,
	makeStyles,
} from "@material-ui/core/styles";
import {
	ProfileState,
	catchHandler,
	fetchProfile,
	selectProfile,
	updateProfile,
} from "../../../slices/profileSlice";
import React, { useEffect, useState } from "react";
import formValidation, {
	ErrorPackage,
} from "../../../constants/forms/Validation";
import { useDispatch, useSelector } from "react-redux";

import Avatar from "@material-ui/core/Avatar";
import Box from "@material-ui/core/Box";
import Button from "@material-ui/core/Button";
import Container from "@material-ui/core/Container";
import Flag from "react-world-flags";
import LockIcon from "@material-ui/icons/Lock";
import MenuItem from "@material-ui/core/MenuItem";
import MoreVertIcon from "@material-ui/icons/MoreVert";
import PublicIcon from "@material-ui/icons/Public";
import TextField from "@material-ui/core/TextField";
import countries from "country-list";

/* Styles */
const useStyles = makeStyles((theme: Theme) => ({
	avatar: {
		marginBottom: "25px",
		marginTop: "25px",
		width: "150px",
		height: "150px",
	},
	container: {
		alignItems: "center",
		backgroundColor: "lightgray",
		borderRadius: 12,
		display: "flex",
		flexDirection: "column",
		padding: 20,
		width: "50%",
		zIndex: 1,
	},
	nameFlagBox: {
		display: "flex",
		flexDirection: "row",
	},
	inputIconBox: {
		display: "flex",
		flexDirection: "row",
		width: "70%",
	},
	bioBox: {
		display: "flex",
		flexDirection: "row",
		width: "66%",
		marginLeft: "-29px"
	},
	item: {
		marginBottom: 12,
		marginLeft: 0,
	},
	page: {
		alignItems: "center",
		display: "flex",
		flexDirection: "column",
		height: "100%",
		justifyContent: "center",
		left: 0,
		position: "relative",
		top: 0,
		width: "100%",
	},
	textContainer: {
		display: "flex",
		justifyContent: "left",
		flexDirection: "row",
	},
	textField: {
		background: "white",
		marginBottom: 12,
		width: "90%",
	},
	flag: {
		marginLeft: "25px",
		marginTop: "5px",
		width: "50px",
		height: "50px",
	},
	icon: {
		alignItems: "center",
		height: "30px",
		marginLeft: "10px",
		width: "30px",
	},
	dropDown: {
		background: "white",
		marginBottom: 12,
		width: "90%",
	},
    titleContainer: {
        marginBottom: 12,
    },
}));

export default function EditProfileHeader() {
	// ****** Components & Hooks ******
	const classes = useStyles();
	const user = useSelector(selectProfile);
	const [state, setState] = React.useState<Partial<ProfileState>>(user);
	const dispatch = useDispatch();
	useEffect(() => {
		fetchProfile(user.user_id, (e) => dispatch(catchHandler(e))).then(
			(result) => {
				if (result) setState(result);
			}
		);
	}, [user.user_id, dispatch]);

	// Maintains error messages for entries
	const [errors, setErrors] = useState<Partial<ErrorPackage>>({
		phone: "",
		email: "",
	});
    // Overwrite some country code to make the flag show up properly
    countries.overwrite([
        {code : "US", name : "United States"},
        {code : "GB", name : "United Kingdom"},
    ]);
    // Privacy Menu Open hooks
    const [isPhonePrivacyMenuOpened , setPhonePrivacyMenuOpen ] = useState(false);
    const [isEmailPrivacyMenuOpened , setEmailPrivacyMenuOpen ] = useState(false);
    const [isYearPrivacyMenuOpened , setYearPrivacyMenuOpen ] = useState(false);
    const [isCollegePrivacyMenuOpened , setCollegePrivacyMenuOpen ] = useState(false);
    const [isCountryPrivacyMenuOpened , setCountryPrivacyMenuOpen ] = useState(false);
    
    // ****** Functional Components ******
	// Bio
	const maxChars = 200;
	const [charCount, setCharCount] = useState(state.bio ? maxChars - state.bio.length : maxChars);

	// Runs every time any field experiences a change in value
	const handleChange = (event: any) => {
		const { name, value } = event.target;
		if (name === "bio") {
			setCharCount(maxChars - value.length);
		}
		setState((prevState) => ({
			...prevState,
			[name]: value,
		}));
		validate({ [name]: value });
	};

	// Dispatch profile update call on "Save Changes" press
	const onSaveClick = () => {
		if (validate())
			dispatch(
				updateProfile(state, () => (window.location.href = "/Profile"))
			);
	};

	// Validates field inputs and provides error messages
	const validate = (field = state) => {
		let temp = { ...errors };
		if (!("phone" in field) || field === state)
			temp.phone = formValidation("phone", state.phone!);
		if (!("email" in field) || field === state)
			temp.email = formValidation("email", state.email!);
		setErrors(temp);

		return Object.values(temp).every((x) => x === "");
	};

	// const [list , setList] = useState(["Public", "Private"]);
	const toggleDropDown = (id: string) => {
		switch (id) {
			case "phone":
				setPhonePrivacyMenuOpen(!isPhonePrivacyMenuOpened);
				break;
			case "email":
				setEmailPrivacyMenuOpen(!isEmailPrivacyMenuOpened);
				break;
			case "year":
				setYearPrivacyMenuOpen(!isYearPrivacyMenuOpened);
				break;
			case "country":
				setCountryPrivacyMenuOpen(!isCountryPrivacyMenuOpened);
				break;
			case "college":
				setCollegePrivacyMenuOpen(!isCollegePrivacyMenuOpened);
				break;
		}
	};

	// Sets privacy selection changes
	const setPrivate = (isPrivate: boolean, id: string) => {
		switch (id) {
			case "phone":
				setState((prevState) => ({
					...prevState,
					isPhonePublic: !isPrivate,
				}));
				break;
			case "email":
				setState((prevState) => ({
					...prevState,
					isEmailPublic: !isPrivate,
				}));
				break;
			case "year":
				setState((prevState) => ({
					...prevState,
					isYearPublic: !isPrivate,
				}));
				break;
			case "country":
				setState((prevState) => ({
					...prevState,
					isCountryPublic: !isPrivate,
				}));
				break;
			case "college":
				setState((prevState) => ({
					...prevState,
					isResidentialCollegePublic: !isPrivate,
				}));
				break;
		}
	};

	// ****** UI Component(s) ******

	const PrivacySettingMenu = (id: string) => {
		return (
			<Menu
				id={id}
				open={true}
				anchorEl={document.getElementById(id)}
				keepMounted
				getContentAnchorEl={null}
				anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
				transformOrigin={{ vertical: "top", horizontal: "left" }}
			>
				<MenuItem value="1" onClick={() => setPrivate(false, id)}>
					Public
				</MenuItem>
				<MenuItem value="2" onClick={() => setPrivate(true, id)}>
					Private
				</MenuItem>
			</Menu>
		);
	};

	const textFieldTheme = createMuiTheme({
		props: {
			MuiTextField: {
				className: classes.textField,
				fullWidth: true,
				// required: true,
				size: "small",
				variant: "outlined",
				onChange: handleChange,
			},
		},
	});
	return (
		<div className={classes.page}>
			<Avatar className={classes.avatar} src={state.pictureUrl} />

			<Box className={classes.nameFlagBox}>
				{/* Name */}
				<Typography className={classes.item} variant="h3">
					{state.firstName + " " + state.lastName}
				</Typography>
				{/* Flag icon (just US for now) */}
				<Flag
					code={countries.getCode(state.country || "")}
					className={classes.flag}
				/>
			</Box>

			<Container className={classes.container}>
				<MuiThemeProvider theme={textFieldTheme}>
                    <div className={classes.titleContainer}>
                        <Typography variant={"h5"}>Profile</Typography>
                    </div>
					<Box className={classes.inputIconBox}>
						{/* Phone Number */}
						<TextField
							label="Phone #"
							value={state.phone || ""}
							name="phone"
							error={errors.phone !== ""}
							helperText={errors.phone}
						/>
						{!state.isPhonePublic ? (
							<LockIcon className={classes.icon}></LockIcon>
						) : (
							<PublicIcon className={classes.icon}></PublicIcon>
						)}
						<IconButton
							id="phone"
							className={classes.icon}
							onClick={() => toggleDropDown("phone")}
						>
							<MoreVertIcon></MoreVertIcon>
							{isPhonePrivacyMenuOpened
								? PrivacySettingMenu("phone")
								: false}
						</IconButton>
					</Box>
					<Box className={classes.inputIconBox}>
						{/* Email */}
						<TextField
							label="Email"
							value={state.email || ""}
							name="email"
							error={errors.email !== ""}
							helperText={errors.email}
						/>
						{!state.isEmailPublic ? (
							<LockIcon className={classes.icon}></LockIcon>
						) : (
							<PublicIcon className={classes.icon}></PublicIcon>
						)}
						<IconButton
							id="email"
							className={classes.icon}
							onClick={() => toggleDropDown("email")}
						>
							<MoreVertIcon></MoreVertIcon>
							{isEmailPrivacyMenuOpened
								? PrivacySettingMenu("email")
								: false}
						</IconButton>
					</Box>
					<Box className={classes.inputIconBox}>
						{/* School year*/}
						<TextField
							// We need to standardize year vals with backend for this not to cause error
							// defaultValue={user.year || ""}
							label="Year"
							name="year"
							id="select"
							value={state.year || ""}
							select
						>
							{YEARS.map((year) => (
								<MenuItem value={year} key={year}>
									{year}
								</MenuItem>
							))}
						</TextField>
						{!state.isYearPublic ? (
							<LockIcon className={classes.icon}></LockIcon>
						) : (
							<PublicIcon className={classes.icon}></PublicIcon>
						)}
						<IconButton
							id="year"
							className={classes.icon}
							onClick={() => toggleDropDown("year")}
						>
							<MoreVertIcon></MoreVertIcon>
							{isYearPrivacyMenuOpened
								? PrivacySettingMenu("year")
								: false}
						</IconButton>
					</Box>
					<Box className={classes.inputIconBox}>
						{/* Country*/}
						<TextField
							// We need to standardize country vals with backend for this not to cause error
							// defaultValue={user.country || ""}
							label="College"
							name="college"
							id="select"
							value={state.college || ""}
							select
						>
							{COLLEGES.map((college) => (
								<MenuItem value={college} key={college}>
									{college}
								</MenuItem>
							))}
						</TextField>
						{!state.isResidentialCollegePublic ? (
							<LockIcon className={classes.icon}></LockIcon>
						) : (
							<PublicIcon className={classes.icon}></PublicIcon>
						)}
						<IconButton
							id="college"
							className={classes.icon}
							onClick={() => toggleDropDown("college")}
						>
							<MoreVertIcon></MoreVertIcon>
							{isCollegePrivacyMenuOpened
								? PrivacySettingMenu("college")
								: false}
						</IconButton>
					</Box>
					<Box className={classes.inputIconBox}>
						{/* College*/}
						<TextField
							// We need to standardize country vals with backend for this not to cause error
							// defaultValue={user.country || ""}
							label="Country"
							name="country"
							id="select"
							value={state.country || ""}
							select
						>
							{COUNTRIES.map((country) => (
								<MenuItem value={country} key={country}>
									{country}
								</MenuItem>
							))}
						</TextField>
						{!state.isCountryPublic ? (
							<LockIcon className={classes.icon}></LockIcon>
						) : (
							<PublicIcon className={classes.icon}></PublicIcon>
						)}
						<IconButton
							id="country"
							className={classes.icon}
							onClick={() => toggleDropDown("country")}
						>
							<MoreVertIcon></MoreVertIcon>
							{isCountryPrivacyMenuOpened
								? PrivacySettingMenu("country")
								: false}
						</IconButton>
					</Box>
					<Box className={classes.bioBox}>
						<TextField
							inputProps={{maxLength: maxChars}}
							label={"Bio"}
							helperText={"Remaining characters: " + charCount}
							value={state.bio || ""}
							name="bio"
							multiline={true}
							rows={3}
						/>
					</Box>
					<Button
						color="primary"
						onClick={onSaveClick}
						variant="contained"
					>
						Save Changes
					</Button>
				</MuiThemeProvider>
			</Container>
		</div>
	);
}
