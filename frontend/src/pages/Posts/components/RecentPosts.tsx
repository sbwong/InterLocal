import CircularProgress from "@material-ui/core/CircularProgress";
import Grid from "@material-ui/core/Grid";
import React, { useEffect , useState } from "react";
import { Theme, makeStyles } from "@material-ui/core/styles";
import { Typography } from "@material-ui/core";
import { useDispatch, useSelector } from "react-redux";
import {
    Post,
    fetchRecentPosts,
    fetchAllPostCount,
    selectRecentPosts,
} from "../../../slices/postSlice";
import RecentPostPreviewBox from "./RecentPostPreviewBox";

const TOP_POSTS_NUM = 3;

type TopPostProps = {
    className?: string;
};

const useStyles = makeStyles((theme: Theme) => ({
    postContainer: {
        backgroundColor: "#F7F7F7",
        flexDirection: "column",
        width: "80%",
        display: "flex",
        flexWrap: "wrap",
        paddingBottom: "20px",
        marginRight: "auto",
    },
    loading: {
        visibility: "hidden",
    },
}));

export default function RecentPosts(props: TopPostProps) {
    const classes = useStyles();
    const dispatch = useDispatch();
    const recentPosts = useSelector(selectRecentPosts);
    const [isFetching, setIsFetching] = useState(true);
    const [postCount, setPostCount] = useState(0);
    const existsPosts = postCount !== 0;

    useEffect(() => {
        (async () => {
            await fetchAllPostCount().then((result) => {
                setPostCount(result);
            });
            await dispatch(fetchRecentPosts(TOP_POSTS_NUM));
            setIsFetching(false);
        })();
    }, [dispatch]);

    return isFetching ? (
            <CircularProgress className={classes.loading} />
    ) : (
            existsPosts ? (
            <Grid container className={props.className || classes.postContainer}>
                <Grid container direction="column" alignItems="center">
                    <Typography variant="h5">
                        Recent Posts
                    </Typography>
                    {/* Read posts from store and load them into components */}
                    {recentPosts.map((post: Post) => {
                        return (
                            <RecentPostPreviewBox
                                {...post}
                                key={post.post_id}
                            ></RecentPostPreviewBox>
                        );
                    })}
                </Grid>

                </Grid >
            ) : (
                <Grid container justify="center">
                    <Typography>No posts yet.</Typography>
                </Grid>
            )
    );
}
