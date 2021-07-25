import { Theme, makeStyles } from "@material-ui/core/styles";

import Avatar from "@material-ui/core/Avatar";
import React from "react";

const useStyles = makeStyles((theme: Theme) => ({
    // For use on the navbar
    avatarSmall: {
        color: theme.palette.getContrastText("#F39327"),
        backgroundColor: "#F39327",
    },
    // For use on the profile page
    avatarLarge: {
        width: "200px",
        height: "200px",
        marginBottom: "25px",
        fontSize: "72px",
        color: theme.palette.getContrastText("#F39327"),
        backgroundColor: "#F39327",
    },
}));

export interface AvatarIconProps {
    isSmall: boolean;
    initials: string;
}

export default function AvatarIcon(props: AvatarIconProps) {
    const classes = useStyles();

    return (
        <div>
            {/* Display the user's initials on the navbar if the user has initials. */}
            {props.isSmall && props.initials.length > 0 && (
                <Avatar className={classes.avatarSmall}>
                    {props.initials}
                </Avatar>
            )}
            {/* Display an avatar icon on the navbar if the user does not have initials. */}
            {props.isSmall && props.initials.length === 0 && (
                <Avatar className={classes.avatarSmall} />
            )}
            {/* Display the user's initials on the profile page if the user has initials */}
            {!props.isSmall && props.initials.length > 0 && (
                <Avatar className={classes.avatarLarge}>
                    {props.initials}
                </Avatar>
            )}
            {/* Display an avatar icon on the profile page if the user does not have initials */}
            {!props.isSmall && props.initials.length === 0 && (
                <Avatar className={classes.avatarLarge} />
            )}
        </div>
    );
}
