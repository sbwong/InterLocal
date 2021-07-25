import { Comment, sendCommentDeletion } from "../../../slices/postSlice";
import PostedByLabel, {
	PostedByLabelProps,
} from "../../../common/PostedByLabel";
import React, { useState } from "react";
import { Theme, makeStyles } from "@material-ui/core/styles";
import { selectIsAdmin, selectUserID } from "../../../slices/profileSlice";
import { useDispatch, useSelector } from "react-redux";

import DeleteDialog from "./DeleteDialog";
import EditCommentInputBox from "../components/EditCommentBox";
import EditIcon from "@material-ui/icons/Edit";
import IconButton from "@material-ui/core/IconButton";
import { Typography } from "@material-ui/core";
import UpvoteDownvote from "./UpvoteDownvote";

/* Styles */
const useStyles = makeStyles((theme: Theme) => ({
	box: {
		backgroundColor: "lightgray",
		borderRadius: 12,
		display: "flex",
		flexDirection: "row",
		padding: "3vh",
		width: "auto",
		marginTop: "4vh",
		justifyContent: "space-between",
	},
	votingBox: {
		display: "flex",
		flexDirection: "row",
		alignItems: "flex-start",
	},
	subBox: {
		display: "flex",
		flexDirection: "column",
	},
	btnBox: {
		display: "flex",
		flexDirection: "row",
		alignItems: "center",
	},
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

interface CommentBoxProps {
	comment: Comment;
	errorHandler: () => void;
}

const CommentBox: React.FC<CommentBoxProps> = ({ comment, errorHandler }) => {
	const classes = useStyles();
	const loggedinUserID = useSelector(selectUserID);
	const isAdmin = useSelector(selectIsAdmin);
	const isChangeable =
		comment === undefined ||
		comment.author_id === loggedinUserID ||
		isAdmin;
	const [state] = useState<Comment>(comment);
	const [editState, setEditState] = useState<Boolean>(false);
	const dispatch = useDispatch();
	console.log("isChangeable: ", isChangeable);

	const onDeleteClick = async () => {
		console.log("Delete button clicked!", state);
		// TODO: validation
		dispatch(
			sendCommentDeletion(
				state,
				() => (window.location.href = "/Post/" + state.post_id)
			)
		);
	};

	const onEditClick = () => {
		setEditState(!editState);
	};

	let editCommentBody = (
		<EditCommentInputBox
			postID={comment.post_id}
			contentBody={comment.content_body}
			commentID={comment.comment_id}
			createdTime={comment.created_time}
			setEditState={setEditState}
		/>
	);
	let commentBody = (
		<Typography className={classes.item} variant="body1">
			{comment.content_body}
		</Typography>
	);

	let props: PostedByLabelProps = {
		author_id: comment.author_id,
		username: comment.username,
		created_time: comment.created_time,
		last_edit_time: comment.last_edit_time,
		is_comment: true,
		is_admin: comment.is_admin,
	};

	// TODO: fetch actual username from the API using the user_id
	return (
		<Typography component={"span"} className={classes.box}>
			<div className={classes.votingBox}>
				<div>
					<UpvoteDownvote
						post={comment}
						errorHandler={errorHandler}
					></UpvoteDownvote>
				</div>
				<div className={classes.subBox}>
					{editState ? editCommentBody : commentBody}
					<PostedByLabel {...props} />
				</div>
			</div>
			{isChangeable && (
				<div className={classes.btnBox}>
					<IconButton onClick={onEditClick}>
						<EditIcon />
					</IconButton>
					<DeleteDialog
						successCallback={onDeleteClick}
						isIcon={true}
					/>
				</div>
			)}
		</Typography>
	);
};

export default CommentBox;
