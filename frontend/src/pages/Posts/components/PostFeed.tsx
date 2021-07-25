import {
	Post,
	fetchPostCount,
	fetchPosts,
	selectParams,
	selectPosts,
	selectPostCount,
} from "../../../slices/postSlice";
import React, { useEffect, useState } from "react";
import { Theme, makeStyles } from "@material-ui/core/styles";
import { useDispatch, useSelector } from "react-redux";

import CircularProgress from "@material-ui/core/CircularProgress";
import CreatePostFAB from "../../../common/CreatePostFAB";
import Grid from "@material-ui/core/Grid";
import Pagination from "@material-ui/lab/Pagination";
import PostPreviewBox from "./PostPreviewBox";
import { Typography } from "@material-ui/core";

type PostFeedProps = {
	className?: string;
	tag?: string; // if tag exists, fetch recent posts with that tag
	userID?: number; // if userID exists, fetch recent posts from that user
};

const POSTS_PER_PAGE = 10;

const useStyles = makeStyles((theme: Theme) => ({
    loading: {
        marginTop: "20px",
    },
    postContainer: {
		backgroundColor: "#F7F7F7",
    },
    postHeader: {
		marginTop: "25px",
		marginLeft: "auto",
		marginRight: "auto"
    },
    pagination: {
        marginBottom: "14px",
        marginTop: "14px",
	},
	postFeed: {
		spacing: 1
    }
}));

export default function PostFeed(props: PostFeedProps) {
	const classes = useStyles();
	const dispatch = useDispatch();
	const posts = useSelector(selectPosts);
	const params = useSelector(selectParams);
	const [isFetching, setIsFetching] = useState(true);
	const existsTag = props.tag !== undefined;
	const existsUserID = props.userID !== undefined;
	const postCount = useSelector(selectPostCount);
	const existsPosts = postCount !== 0;
	
	useEffect(() => {
		(async () => {
			if (existsUserID) {
				console.log("A");
				await dispatch(fetchPostCount(props.userID));
				await dispatch(fetchPosts(POSTS_PER_PAGE, 0, props.userID));
			} else if (existsTag) {
				console.log("B");
				await dispatch(fetchPostCount(undefined, [props.tag] as string[]));
				await dispatch(
					fetchPosts(POSTS_PER_PAGE, 0, undefined, [props.tag] as string[])
				);
			//} else if (existsParams) {
			//	console.log("C");
			//	await dispatch(fetchPostCount(params["author_id"], params["tags"], params["user_type"], params["content_type"], params["date_type"]));
			//	await dispatch(fetchPosts(POSTS_PER_PAGE, 0, params["author_id"], params["tags"], true, params["user_type"], params["content_type"], params["date_type"]));
			} else {
				console.log("D");
				await dispatch(fetchPostCount());
				await dispatch(fetchPosts(POSTS_PER_PAGE, 0, undefined, undefined, true));
            }
			
			setIsFetching(false);
		})();
	}, [existsTag, existsUserID, dispatch, props.tag, props.userID]);

	const onPaginationChange = (
		event: React.ChangeEvent<unknown>,
		page: number
	) => {
		if (existsUserID) {
			console.log("A!");
			dispatch(
				fetchPosts(
					POSTS_PER_PAGE,
					(page - 1) * POSTS_PER_PAGE,
					props.userID
				)
			);
		} else if (existsTag) {
			console.log("B!");
			dispatch(
				fetchPosts(
					POSTS_PER_PAGE,
					(page - 1) * POSTS_PER_PAGE,
					undefined,
					[props.tag] as string[]
				)
			);
		} else if (params !== undefined) {
			console.log("C!");
			// dispatch(fetchPostCount(params["author_id"], params["tags"], params["user_type"], params["content_type"], params["date_type"]));
			dispatch(fetchPosts(POSTS_PER_PAGE, (page - 1) * POSTS_PER_PAGE, params["author_id"], params["tags"], true, params["user_type"], params["content_type"], params["date_type"]));
		} else {
			console.log("D!");
			dispatch(fetchPosts(POSTS_PER_PAGE, (page - 1) * POSTS_PER_PAGE, undefined, undefined, true));
		}
		window.scrollTo({
			behavior: "smooth",
			top: 0,
		});
	};

	return isFetching ? (
		<CircularProgress className={classes.loading} />
	) : (
			<Grid container className={classes.postFeed}>
				<CreatePostFAB />
				<Grid
					container
					className={props.className || classes.postContainer}
				>
					<Grid container justify="center">
						<Typography className={classes.postHeader} variant="h5">
							{"Top " +
								(existsTag ? '"' + props.tag + '" ' : "") +
								"Posts"}
						</Typography>
					</Grid>

					{existsPosts ? (
						<Grid container direction="column" alignItems="center">
							{/* Read posts from store and load them into components */}
							{posts.map((post: Post) => {
								return (
									<PostPreviewBox
										{...post}
										key={post.post_id}
									></PostPreviewBox>
								);
							})}
							<Pagination
								className={classes.pagination}
								count={Math.ceil(postCount / POSTS_PER_PAGE)}
								onChange={onPaginationChange}
							/>
						</Grid>
					) : (
						<Grid container justify="center">
							<Typography>No posts yet.</Typography>
						</Grid>
					)}
				</Grid>
			</Grid>
	);
}
