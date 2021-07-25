import { IconButton, TextField, Typography } from "@material-ui/core";
import {
	MuiThemeProvider,
	Theme,
	createMuiTheme,
	makeStyles,
} from "@material-ui/core/styles";
import React, { useState } from "react";
import {
	UpdatePasswordPackage,
	updatePassword,
} from "../../../slices/profileSlice";

import Box from "@material-ui/core/Box";
import Button from "@material-ui/core/Button";
import Container from "@material-ui/core/Container";
import LockIcon from "@material-ui/icons/Lock";
import formValidation from "../../../constants/forms/Validation";
import { useDispatch } from "react-redux";

/* Styles */
const useStyles = makeStyles((theme: Theme) => ({
	container: {
		alignItems: "center",
		backgroundColor: "lightgray",
		borderRadius: 12,
		display: "flex",
		flexDirection: "column",
        marginTop: "25px",
		padding: 20,
		width: "50%",
		zIndex: 1,
	},
	inputIconBox: {
		display: "flex",
		flexDirection: "row",
		width: "70%",
	},
	textField: {
		background: "white",
		marginBottom: 12,
		width: "90%",
	},
	icon: {
		alignItems: "center",
		height: "30px",
		marginLeft: "10px",
		width: "30px",
	},
    titleContainer: {
        marginBottom: 12,
    },
}));

export default function EditPasswordHeader() {
	// ****** Components & Hooks ******
	const classes = useStyles();
    const [state, setState] = useState<Partial<UpdatePasswordPackage>>({
        currentPassword: "",
        newPassword: "",
        confirmNewPassword: "",
    });
	const dispatch = useDispatch();

	// Maintains error messages for entries
	const [errors, setErrors] = useState<Partial<UpdatePasswordPackage>>({
        currentPassword: "",
        newPassword: "",
        confirmNewPassword: "",
	});

	// Runs every time any field experiences a change in value
	const handleChange = (event: any) => {
		const { name, value } = event.target;
        setState((prevState) => ({
            ...prevState,
            [name]: value
        }));
		validate({ [name]: value });
	};
	// Dispatch password update call on "Save Changes" press
	const onSaveClick = () => {
		if (validate())
			dispatch(updatePassword(state, () => (window.location.href = "/Profile")));
	};
	// Validates field inputs and provides error messages
	const validate = (field = state) => {
		let temp = { ...errors };
        if (field.currentPassword !== undefined) {
            temp.currentPassword = formValidation("currentPassword", field.currentPassword);
        }
        if (field.newPassword !== undefined) {
            temp.newPassword = formValidation("password", field.newPassword);
            temp.confirmNewPassword = formValidation("confirmPassword", state.confirmNewPassword || "");
        }
        if (field.confirmNewPassword !== undefined && state.newPassword) {
            temp.confirmNewPassword = state.newPassword !== field.confirmNewPassword ? "Passwords do not match." : "";
        }
		setErrors(temp);
		return Object.values(temp).every((x) => x === "");
	};
	// ****** UI Component(s) ******
	const textFieldTheme = createMuiTheme({
		props: {
			MuiTextField: {
				className: classes.textField,
				fullWidth: true,
				size: "small",
				variant: "outlined",
				onChange: handleChange,
			},
		},
	});
    
	return (
			<Container className={classes.container}>
				<MuiThemeProvider theme={textFieldTheme}>
                    <div className={classes.titleContainer}>
                        <Typography variant={"h5"}>Change Password</Typography>
                    </div>
					<Box className={classes.inputIconBox}>
						{/* Current Password */}
						<TextField
							label="Current Password"
							name="currentPassword"
                            type="password"
							value={state.currentPassword}
							error={errors.currentPassword !== ""}
							helperText={errors.currentPassword}
						/>
						<LockIcon className={classes.icon}></LockIcon>
						<IconButton className={classes.icon} disabled/>
					</Box>
					<Box className={classes.inputIconBox}>
						{/* New Password */}
						<TextField
							label="New Password"
							name="newPassword"
                            type="password"
							value={state.newPassword}
							error={errors.newPassword !== ""}
							helperText={errors.newPassword}
						/>
						<LockIcon className={classes.icon}></LockIcon>
						<IconButton className={classes.icon} disabled/>
					</Box>
					<Box className={classes.inputIconBox}>
						{/* Password */}
						<TextField
							label="Confirm New Password"
							name="confirmNewPassword"
                            type="password"
							value={state.confirmNewPassword}
							error={errors.confirmNewPassword !== ""}
							helperText={errors.confirmNewPassword}
						/>
						<LockIcon className={classes.icon}></LockIcon>
						<IconButton className={classes.icon} disabled/>
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
	);
}
