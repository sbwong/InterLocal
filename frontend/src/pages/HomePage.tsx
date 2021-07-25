import { Theme, makeStyles } from "@material-ui/core/styles";
import Grid from "@material-ui/core/Grid";
import FilterBy from "./Posts/components/FilterBy";
import PostFeed from "./Posts/components/PostFeed";
import React from "react";
import RecentPosts from "./Posts/components/RecentPosts";
import TopTags from "./Posts/components/TopTags";


const useStyles = makeStyles((theme: Theme) => ({
	homepage: {
        backgroundColor: "#F7F7F7",
        flexDirection: "row",
	},
    leftSideBar: {
        minWidth: "200px",
        marginLeft: "auto",
        marginRight: "auto",
        marginTop: "50px",
        //backgroundColor: "green"
    },
    rightSideBar: {
        minWidth: "200px",
        marginLeft: "auto",
        marginRight: "auto",
        marginTop: "50px",
        //backgroundColor: "green"
    },
    postfeed: {
        //backgroundColor: "blue"
    }
}));

export default function HomePage() {
    const classes = useStyles();

    return (
        <Grid container className={classes.homepage}>
            <Grid container item xs={3} direction="column" alignItems="center" className={classes.leftSideBar}>
                <FilterBy></FilterBy>
            </Grid>

            <Grid container item xs={6} direction="column" alignItems="center">
                <PostFeed />
            </Grid>

            <Grid container item xs={3} direction="column" alignItems="center" className={classes.rightSideBar}>
                <TopTags></TopTags>
                <RecentPosts></RecentPosts>
            </Grid>
        </Grid>
    );
}
