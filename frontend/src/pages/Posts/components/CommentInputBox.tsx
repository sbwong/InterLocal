import { Comment, createComment } from "../../../slices/postSlice";
import { Theme, makeStyles } from "@material-ui/core/styles";
import {
	catchHandler,
	selectIsAdmin,
	selectUserID,
	selectUsername,
} from "../../../slices/profileSlice";
import { useDispatch, useSelector } from "react-redux";

import Button from "@material-ui/core/Button";
import React from "react";
import TextField from "@material-ui/core/TextField";

/* Styles */
const useStyles = makeStyles((theme: Theme) => ({
	item: {
		marginBottom: 12,
	},
	textField: {
		width: "80%",
		marginBottom: 10,
	},
	root: {
		"& > *": {
			margin: theme.spacing(2),
		},
	},
}));

export interface CommentInputBoxProps {
	postID: number;
	createCommentHandler: (newComment: Comment | undefined) => void;
}

export default function CommentInputBox(props: CommentInputBoxProps) {
	const classes = useStyles();
	const userID = useSelector(selectUserID);
	const username = useSelector(selectUsername);
	const isAdmin = useSelector(selectIsAdmin);
	const dispatch = useDispatch();
	const [state, setState] = React.useState({
		author_id: userID,
		comment_id: 0, // will be updated when API request is made
		content_body: "",
		created_time: "",
		last_edit_time: "",
		down_count: 0,
		is_post: false,
		is_upvote: false,
		post_id: props.postID,
		up_count: 0,
		username: username,
		is_admin: isAdmin,
	});

	const onCommentChange = (event: any) => {
		const { name, value } = event.target;
		setState((prevState) => ({
			...prevState,
			[name]: value,
		}));
	};
	const onCancelClick = () => {
		setState((prevState) => ({
			...prevState,
			content_body: "",
		}));
	};

	const onSubmitClick = async () => {
		// TODO: the timestamp is created by the frontend side, which might be slightly different from the
		// timestamp stored in the database
		setState((prevState) => ({
			...prevState,
			content_body: "",
			timestamp: Date.now(),
		}));
		console.log("Submit button clicked!", state);
		const comment = await createComment(state, (e) =>
			dispatch(catchHandler(e))
		);
		props.createCommentHandler(comment);
	};
	return (
		<div>
			<TextField
				fullWidth
				multiline
				rows={3}
				placeholder="Comment"
				variant="outlined"
				onChange={onCommentChange}
				value={state.content_body}
				name="content_body"
			/>

			<div className={classes.root}>
				<Button
					color="primary"
					disabled={!state.content_body}
					onClick={onSubmitClick}
					variant="contained"
				>
					Submit
				</Button>
				<Button
					color="secondary"
					disabled={!state.content_body}
					onClick={onCancelClick}
					variant="contained"
				>
					Cancel
				</Button>
			</div>
		</div>
	);
}
