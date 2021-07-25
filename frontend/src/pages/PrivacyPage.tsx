import { Theme, createStyles, makeStyles } from "@material-ui/core/styles";

import Box from "@material-ui/core/Box";
import React from "react";
import { Typography } from "@material-ui/core";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        mainHeader: {
            marginBottom: "25px",
        },
        page: {
            paddingTop: "30px",
            marginBottom: "80px",
            marginTop: "50px",
            justifyContent: "center",
            paddingLeft: "100px",
            paddingRight: "100px",
            textAlign: "left",
        },
        secondaryHeader: {
            marginBottom: "5px",
        },
        bodyText: {
            marginBottom: "25px",
        },
    })
);

export function PrivacyPage() {
    const classes = useStyles();
    return (
        <Box className={classes.page}>
            <Typography variant="h3" className={classes.mainHeader}>
                Privacy Policy
            </Typography>

            <Typography variant="h5" className={classes.secondaryHeader}>
                Information Collection
            </Typography>

            <Typography variant="h6" className={classes.bodyText}>
                When you sign up to use InterLocal, we collect and store the
                following pieces of information:
                <ul>
                    <li>Your first and last name</li>
                    <li>Your username and password</li>
                    <li>Your phone number (optional)</li>
                    <li>Your email address</li>
                    <li>Your home country</li>
                    <li>
                        Your graduation year and residential college, if you are
                        a Rice student
                    </li>
                </ul>
                Your password will always be kept private. Your phone number,
                email address, and graduation year will initially be kept
                private but can be made public if you choose. Your residential
                college and home country will initially be public but can be
                made private if you choose. We do not sell or give out any of
                your private information.
            </Typography>

            <Typography variant="h5" className={classes.secondaryHeader}>
                Cookies
            </Typography>

            <Typography variant="h6" className={classes.bodyText}>
                Our application makes use of third party cookies to enhance the
                user experience. Additionally, our application makes use of
                cookies from Google’s Search API to enable users to search for
                posts. If your browser automatically blocks third party cookies,
                you may not be able to view certain information such as user
                profiles, and you may not be able to create, edit, delete, or
                search for posts.
            </Typography>
        </Box>
    );
}
