import React, { SyntheticEvent, useState } from "react";
import Snackbar, { SnackbarCloseReason } from "@material-ui/core/Snackbar";
import { Theme, createStyles, makeStyles } from "@material-ui/core/styles";

import Alert from "@material-ui/lab/Alert";
import Backdrop from "@material-ui/core/Backdrop";
import Button from "@material-ui/core/Button";
import { ChangeEvent } from "react";
import CircularProgress from "@material-ui/core/CircularProgress";
import Container from "@material-ui/core/Container";
import Link from "@material-ui/core/Link";
import TextField from "@material-ui/core/TextField";
import Typography from "@material-ui/core/Typography";
import { login } from "../../slices/profileSlice";
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
            width: "100%",
            minHeight: "85vh",
        },
        textContainer: {
            justifyContent: "center",
            display: "flex",
            flexDirection: "row",
        },
        textField: {
            marginBottom: 12,
            width: "60%",
        },
    })
);

export default function Login() {
    const classes = useStyles();
    const dispatch = useDispatch();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [errorMsg, setErrorMsg] = useState("");
    const [isSnackbarOpen, setIsSnackbarOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const onLoginClick = () => {
        setIsLoading(true);
        dispatch(
            login(
                username,
                password,
                () => {
                    setIsLoading(false);
                },
                () => {
                    setErrorMsg(
                        "Login failed. Your username or password is incorrect."
                    );
                    setIsSnackbarOpen(true);
                }
            )
        );
    };

    const onUsernameChange = (e: ChangeEvent<HTMLInputElement>) => {
        setUsername(e.currentTarget.value);
    };

    const onPasswordChange = (e: ChangeEvent<HTMLInputElement>) => {
        setPassword(e.currentTarget.value);
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

    const isButtonEnabled = username !== "" && password !== "";

    return (
        <div className={classes.page}>
            <Backdrop className={classes.backdrop} open={isLoading}>
                <CircularProgress color="inherit" />
            </Backdrop>
            <Container className={classes.container}>
                <Typography className={classes.item} variant="h4">
                    Login
                </Typography>
                <TextField
                    className={classes.textField}
                    fullWidth={true}
                    label="Username"
                    onChange={onUsernameChange}
                    required={true}
                    size="small"
                    variant="outlined"
                />
                <TextField
                    className={classes.textField}
                    fullWidth={true}
                    label="Password"
                    onChange={onPasswordChange}
                    required={true}
                    size="small"
                    type="password"
                    variant="outlined"
                />
                <div className={classes.textContainer}>
                    <Typography variant="subtitle2">
                        Don't have an account?{" "}
                        <Link
                            color="inherit"
                            href="/Signup"
                            variant="subtitle2"
                        >
                            Sign Up.
                        </Link>
                    </Typography>
                </div>
                <Button
                    color="primary"
                    disabled={!isButtonEnabled}
                    onClick={onLoginClick}
                    variant="contained"
                >
                    Log In
                </Button>
                <Snackbar open={isSnackbarOpen} onClose={onSnackbarClose}>
                    <Alert onClose={onSnackbarClose} severity="error">
                        {errorMsg}
                    </Alert>
                </Snackbar>
            </Container>
        </div>
    );
}
