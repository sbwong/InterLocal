import Alert, { Color } from "@material-ui/lab/Alert";
import { COLLEGES, COUNTRIES } from "../../../constants/Constants";
import HelpTooltip, { FieldPrivacy } from "../../../common/HelpTooltip";
import {
	MuiThemeProvider,
	Theme,
	createMuiTheme,
	createStyles,
	makeStyles,
} from "@material-ui/core/styles";
import { ProfileState, newProfile } from "../../../slices/profileSlice";
import React, { SyntheticEvent, useState } from "react";
import Snackbar, { SnackbarCloseReason } from "@material-ui/core/Snackbar";
import formValidation, {
	ErrorPackage,
} from "../../../constants/forms/Validation";

import Backdrop from "@material-ui/core/Backdrop";
import Button from "@material-ui/core/Button";
import CircularProgress from "@material-ui/core/CircularProgress";
import Container from "@material-ui/core/Container";
import Link from "@material-ui/core/Link";
import MenuItem from "@material-ui/core/MenuItem";
import TextField from "@material-ui/core/TextField";
import Typography from "@material-ui/core/Typography";
import { useDispatch } from "react-redux";

const useStyles = makeStyles((theme: Theme) =>
	createStyles({
		backdrop: {
			zIndex: theme.zIndex.drawer + 1,
			color: "#f7f7f7",
		},
		container: {
			alignItems: "center",
			backgroundColor: "lightgray",
			borderRadius: 12,
			display: "flex",
			flexDirection: "column",
			padding: 20,
			width: "30%",
			marginTop: "10vh",
		},
		item: {
			marginBottom: 12,
		},
		page: {
			alignItems: "center",
			display: "flex",
			height: "100%",
			justifyContent: "center",
			left: 0,
			position: "relative",
			top: 0,
			width: "100%",
		},
		row: {
			display: "flex",
			flexDirection: "row",
			alignItems: "center",
			width: "65%",
		},
		textContainer: {
			justifyContent: "center",
			display: "flex",
			flexDirection: "row",
		},
		textField: {
			marginBottom: 12,
			textAlign: "left",
			flexGrow: 1,
			marginRight: theme.spacing(1),
		},
	})
);

export default function SignupBox(this: any) {
	// ****** Components & Hooks ******

	const classes = useStyles();
	const dispatch = useDispatch();

	// Creates hook for state, which is changed by form and dispatched upon completion
	const [state, setState] = useState<Partial<ProfileState>>({
		confirmPassword: "",
		email: "",
		firstName: "",
		lastName: "",
		password: "",
		phone: "",
		username: "",
	});

	// Maintains error messages for entries
	const [errors, setErrors] = useState<Partial<ErrorPackage>>({
		confirmPassword: "",
		email: "",
		firstName: "",
		lastName: "",
		password: "",
		phone: "",
		username: "",
	});

	const isSignUpEnabled =
		state.firstName &&
		state.lastName &&
		state.username &&
		state.email &&
		state.year &&
		state.country &&
		state.college &&
		state.password &&
		state.confirmPassword;

	// ****** Functional Components ******

	// Runs every time any field experiences a change in value
	const handleChange = (e: any) => {
		const { name, value } = e.target;
		validate({ [name]: value });
		setState((prevState) => ({
			...prevState,
			[name]: value,
		}));
	};

	// Dispatch profile creation call on "Sign Up" press
	const onSignUpClick = () => {
		if (validate()) {
			setIsLoading(true);
			dispatch(newProfile(state, showSnackbar));
		}
	};

	// Validates field inputs and provides error messages
	const validate = (field = state) => {
		let temp = { ...errors };
		if (field.firstName)
			temp.firstName = formValidation("firstName", field.firstName);
		if (field.lastName)
			temp.lastName = formValidation("lastName", field.lastName);
		if (field.username)
			temp.username = formValidation("username", field.username);
		if (field.phone) temp.phone = formValidation("phone", field.phone);
		if (field.email) temp.email = formValidation("email", field.email);
		if (field.password)
			temp.password = formValidation("password", field.password);
		if (field.confirmPassword) {
			temp.confirmPassword = formValidation(
				"confirmPassword",
				field.confirmPassword!
			);
		}
		if (field.confirmPassword) {
			temp.confirmPassword =
				state.password === field.confirmPassword
					? ""
					: "Passwords do not match.";
		}

		setErrors(temp);
		return Object.values(temp).every((x) => x === "");
	};

	// ****** UI Component(s) ******

	// Sets Material UI Theme components
	const textFieldTheme = createMuiTheme({
		props: {
			MuiTextField: {
				className: classes.textField,
				fullWidth: true,
				required: true,
				size: "small",
				variant: "outlined",
				onChange: handleChange,
			},
		},
	});

	// States and handlers for coordinating loading screen on signup click
	const [isLoading, setIsLoading] = useState(false);
	const [isSnackbarOpen, setIsSnackbarOpen] = useState(false);
	const [snackbarMessage, setSnackbarMessage] = useState("");
	const [snackbarSeverity, setSnackbarSeverity] = useState<Color>("info"); // Use info as the default severity state

	// Callback for newProfile function to use to display message
	const showSnackbar = (message: string, severity: Color) => {
		// Display snackbar with message and stop showing loading screen
		setIsSnackbarOpen(true);
		setSnackbarMessage(message);
		setSnackbarSeverity(severity);
		setIsLoading(false);
	};
	const onSnackbarClose = (
		event: SyntheticEvent,
		reason?: SnackbarCloseReason
	) => {
		if (reason === "clickaway") {
			// Don't disable snackbar on clickaway to give users time to read the message
			return;
		}
		setIsSnackbarOpen(false);
	};

	return (
		<div className={classes.page}>
			<Backdrop className={classes.backdrop} open={isLoading}>
				<CircularProgress color="inherit" />
			</Backdrop>
			<Container className={classes.container}>
				<Typography className={classes.item} variant="h4">
					Sign Up
				</Typography>
				<MuiThemeProvider theme={textFieldTheme}>
					<div className={classes.row}>
						<TextField
							label="First Name"
							name="firstName"
							value={state.firstName || ""}
							error={errors.firstName !== ""}
							helperText={errors.firstName}
						/>
						<HelpTooltip
							title="Your first name."
							privacy={FieldPrivacy.PUBLIC}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							label="Last Name"
							name="lastName"
							value={state.lastName || ""}
							error={errors.lastName !== ""}
							helperText={errors.lastName}
						/>
						<HelpTooltip
							title="Your last name."
							privacy={FieldPrivacy.PUBLIC}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							label="Username"
							name="username"
							value={state.username || ""}
							error={errors.username !== ""}
							helperText={errors.username}
						/>
						<HelpTooltip
							title="Your username, which will be used to log in."
							privacy={FieldPrivacy.PUBLIC}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							className={classes.textField}
							error={errors.password !== ""}
							fullWidth={true}
							helperText={errors.password}
							label="Password"
							name="password"
							required={true}
							size="small"
							type="password"
							variant="outlined"
							value={state.password || ""}
						/>
						<HelpTooltip
							title="Your password."
							privacy={FieldPrivacy.PRIVATE}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							className={classes.textField}
							error={errors.confirmPassword !== ""}
							fullWidth={true}
							helperText={errors.confirmPassword}
							label="Confirm Password"
							name="confirmPassword"
							required={true}
							size="small"
							type="password"
							variant="outlined"
							value={state.confirmPassword || ""}
						/>
						<HelpTooltip
							title="Confirm your password."
							privacy={FieldPrivacy.PRIVATE}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							label="Phone #"
							name="phone"
							value={state.phone || ""}
							error={errors.phone !== ""}
							helperText={errors.phone}
							required={false}
						/>
						<HelpTooltip
							title="This phone number is not required but can be provided for other users to contact you."
							privacy={FieldPrivacy.INITIAL_PRIVATE}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							label="Email"
							name="email"
							value={state.email || ""}
							error={errors.email !== ""}
							helperText={errors.email}
						/>
						<HelpTooltip
							title="Your email address. This is another method for users to contact you."
							privacy={FieldPrivacy.INITIAL_PRIVATE}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							label="Year"
							name="year"
							id="select"
							select
							value={state.year || ""}
						>
							<MenuItem value={"Freshman"}>Freshman</MenuItem>
							<MenuItem value={"Sophomore"}>Sophomore</MenuItem>
							<MenuItem value={"Junior"}>Junior</MenuItem>
							<MenuItem value={"Senior"}>Senior</MenuItem>
						</TextField>
						<HelpTooltip
							title="Your college year."
							privacy={FieldPrivacy.INITIAL_PRIVATE}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							label="Home Country"
							name="country"
							id="select"
							select
							value={state.country || ""}
						>
							{COUNTRIES.map((country) => (
								<MenuItem value={country} key={country}>
									{country}
								</MenuItem>
							))}
						</TextField>
						<HelpTooltip
							title="Your home country."
							privacy={FieldPrivacy.INITIAL_PUBLIC}
						/>
					</div>
					<div className={classes.row}>
						<TextField
							label="Residential College"
							name="college"
							id="select"
							select
							value={state.college || ""}
						>
							{COLLEGES.map((college) => (
								<MenuItem value={college} key={college}>
									{college}
								</MenuItem>
							))}
						</TextField>
						<HelpTooltip
							title="Your residential college at Rice University."
							privacy={FieldPrivacy.INITIAL_PUBLIC}
						/>
					</div>
				</MuiThemeProvider>
				<div className={classes.textContainer}>
					<Typography variant="subtitle2">
						Already have an account?{" "}
						<Link color="inherit" href="/Login" variant="subtitle2">
							Login.
						</Link>
					</Typography>
				</div>
				<Button
					color="primary"
					disabled={!isSignUpEnabled}
					onClick={onSignUpClick}
					variant="contained"
				>
					Sign Up
				</Button>
			</Container>
			<Snackbar open={isSnackbarOpen} onClose={onSnackbarClose}>
				<Alert onClose={onSnackbarClose} severity={snackbarSeverity}>
					{snackbarMessage}
				</Alert>
			</Snackbar>
		</div>
	);
}
