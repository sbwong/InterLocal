import { Box, Grid, IconButton, Typography } from "@material-ui/core";
import {
	Comment,
	Content,
	cancelUpvoteDownvote,
	doUpvoteDownvote,
} from "../../../slices/postSlice";
import { ExpandLess, ExpandMore } from "@material-ui/icons";
import React, { useEffect, useState } from "react";
import { Theme, makeStyles } from "@material-ui/core/styles";

import { catchHandler } from "../../../slices/profileSlice";
import { useDispatch } from "react-redux";

const useStyles = makeStyles((theme: Theme) => ({
	parent: {
		display: "flex",
		flexDirection: "column",
		alignItems: "center",
	},
	button: {
		paddingTop: 0,
		paddingBottom: 0,
	},
}));
interface UpvoteDownvoteProps {
	post: Content;
	errorHandler: () => void;
}

const UpvoteDownvote = ({ post, errorHandler }: UpvoteDownvoteProps) => {
	const classes = useStyles();
	const [upvotes, setUpvotes] = useState(post.up_count);
	const [downvotes, setDownvotes] = useState(post.down_count);
	const [clickedUpvote, setClickedUpvote] = useState(false);
	const [clickedDownvote, setClickedDownvote] = useState(false);
	const dispatch = useDispatch();
	const dispatchCatchHandler = (e: any) => dispatch(catchHandler(e));
	const isPost = post.is_post;
    const id = isPost?post.post_id:(post as Comment).comment_id;

	useEffect(() => {
		const isUpvote = post.is_upvote;
		if (isUpvote === true) {
			setClickedUpvote(true);
		} else if (isUpvote === false) {
			setClickedDownvote(true);
		}
	}, [post]);

	const doUpvote = async () => {
		if (clickedDownvote) {
			// Post has already been downvoted
			setClickedDownvote(false);
			setDownvotes(downvotes - 1);
		}

		if (!clickedUpvote) {
			if (
				(await doUpvoteDownvote(
					id, isPost, true,
					dispatchCatchHandler
				)) !== undefined
			) {
				setUpvotes(upvotes + 1);
				setClickedUpvote(true);
				return;
			}
		} else {
			// Post has already been upvoted; no upvotes/downvotes now
			if (
				(await cancelUpvoteDownvote(
					id, isPost, true,
					dispatchCatchHandler
				)) !== undefined
			) {
				setClickedUpvote(false);
				setUpvotes(upvotes - 1);
				return;
			}
		}
		// One of the API calls did not work, so handle the error
		errorHandler();
	};

	const doDownvote = async () => {
		if (clickedUpvote) {
			// Post has already been upvoted
			setClickedUpvote(false);
			setUpvotes(upvotes - 1);
		}

		if (!clickedDownvote) {
			if (
				(await doUpvoteDownvote(
					id, isPost, false,
					dispatchCatchHandler
				)) !== undefined
			) {
				setDownvotes(downvotes + 1);
				setClickedDownvote(true);
				return;
			}
		} else {
			// Post has already been downvoted; no upvotes/downvotes now

			if (
				(await cancelUpvoteDownvote(
					id, isPost, false,
					dispatchCatchHandler
				)) !== undefined
			) {
				setDownvotes(downvotes - 1);
				setClickedDownvote(false);
				return;
			}
		}
		// One of the API calls did not work, so handle the error
		errorHandler();
	};

	return (
		<Grid
			container
			direction="column"
			justify="center"
			alignItems="center"
		>
			<IconButton
				onClick={() => {
					doUpvote();
				}}
				color={clickedUpvote ? "secondary" : "primary"}
				className={classes.button}
			>
				<ExpandLess />
			</IconButton>
			
			<Typography>
				<Box fontWeight="medium">{upvotes - downvotes}</Box>
			</Typography>

			<IconButton
				onClick={() => {
					doDownvote();
				}}
				color={clickedDownvote ? "secondary" : "primary"}
				className={classes.button}
			>
				<ExpandMore />
			</IconButton>
		</Grid>
	);
};

export default UpvoteDownvote;
