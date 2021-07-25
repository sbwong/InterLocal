import { Theme, createStyles, makeStyles } from "@material-ui/core/styles";

import Link from "@material-ui/core/Link";
import React from "react";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        footerStyle: {
            borderTop: "1px solid #E7E7E7",
            textAlign: "center",
            padding: "20px",
            marginTop: "25px",
            paddingBottom: "5px",
            bottom: "0",
            width: "100%",
        },
    })
);

export default function Footer() {
    const classes = useStyles();
    return (
        <div>
            <div className="row">
                <p className={classes.footerStyle}>
                    &copy;{new Date().getFullYear()} All rights reserved |{" "}
                    <Link href={"/Privacy"}>Privacy</Link>
                </p>
            </div>
        </div>
    );
}
