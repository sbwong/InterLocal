import { Theme, createStyles, makeStyles } from "@material-ui/core/styles";

import CookieConsent from "react-cookie-consent";
import Link from "@material-ui/core/Link";
import React from "react";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        privacyLink: {
            color: "white",
            textDecoration: "underline",
        },
    })
);

export default function CookieConsentBar() {
    const classes = useStyles();
    return (
        <CookieConsent>
            This website uses third party cookies to enhance the user
            experience. For more information, please visit our{" "}
            <Link href={"/Privacy"} className={classes.privacyLink}>
                privacy page.
            </Link>
        </CookieConsent>
    );
}
