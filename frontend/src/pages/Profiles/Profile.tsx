import { Theme, makeStyles } from "@material-ui/core/styles";

import { Divider } from "@material-ui/core";
import PostFeed from "../Posts/components/PostFeed";
import ProfileHeader from "./components/ProfileHeader";
import React from "react";
import { selectUserID } from "../../slices/profileSlice";
import { useSelector } from "react-redux";

const useStyles = makeStyles((theme: Theme) => ({
    postFeed: {
        marginTop: "10px",
    },
    profileHeader: {
        marginTop: "50px",
        marginBottom: "20px",
    },
	page: {
		alignItems: "top",
		display: "flex",
		flexDirection: "row",
		height: "100%",
		justifyContent: "center",
		left: 0,
		position: "relative",
		top: 0,
		width: "100%",
	}
}));

export function Profile(props: any) {
    const classes = useStyles();
    const myUserID = useSelector(selectUserID);
    const pathUserID = parseInt(props.match?.params.user_id);

    // In the case that there was no path parameter (ex: localhost3000/Profile), the pathUserID
    // will be NaN. Otherwise, it will be the number right after the word Profile (ex: localhost3000/Profile7
    // will have a profile_user_id of 7).
    var userID: number;
    if (!isNaN(pathUserID)) {
        userID = pathUserID;
    } else {
        userID = myUserID;
    }

    console.log("bruh")

    return (
		<div className={classes.page}>
            <ProfileHeader className={classes.profileHeader} userID={userID} />
            <Divider variant={"middle"} />
            <PostFeed className={classes.postFeed} userID={userID} />
        </div>
    );
}
