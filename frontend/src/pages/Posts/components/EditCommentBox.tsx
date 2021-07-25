import { Theme, makeStyles } from "@material-ui/core/styles";
import { selectUserID, selectUsername } from "../../../slices/profileSlice";
import { useDispatch, useSelector } from "react-redux";

import Button from "@material-ui/core/Button";
import React from "react";
import TextField from "@material-ui/core/TextField";
import { sendCommentUpdate } from "../../../slices/postSlice";

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

export interface EditCommentInputBoxProps {
	postID: number;
	contentBody: string;
	commentID: number;
	setEditState: Function;
	createdTime: string;
}

export default function EditCommentInputBox(props: EditCommentInputBoxProps) {
	const classes = useStyles();
	const userID = useSelector(selectUserID);
	const username = useSelector(selectUsername);
	const [state, setState] = React.useState({
		author_id: userID,
		comment_id: props.commentID,
		content_body: props.contentBody,
		created_time: "",
		down_count: 0,
		is_post: false,
		is_upvote: false,
		post_id: props.postID,
		up_count: 0,
		username: username,
		last_edit_time: "",
		is_admin: false,
	});
	const dispatch = useDispatch();

	const onCommentChange = (event: any) => {
		const { name, value } = event.target;
		setState((prevState) => ({
			...prevState,
			[name]: value,
		}));
	};
	const onCancelClick = () => {
		props.setEditState(false);
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
		dispatch(
			sendCommentUpdate(
				state,
				() => (window.location.href = "/Post/" + state.post_id)
			)
		);
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
