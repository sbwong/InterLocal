import { Post, sendUpdate } from "../../../slices/postSlice";
import React, {
	ChangeEvent,
	KeyboardEvent,
	SyntheticEvent,
	useState,
} from "react";
import Snackbar, { SnackbarCloseReason } from "@material-ui/core/Snackbar";
import { Theme, makeStyles } from "@material-ui/core/styles";
import { selectIsAdmin, selectUserID } from "../../../slices/profileSlice";
import { useDispatch, useSelector } from "react-redux";

import Alert from "@material-ui/lab/Alert";
import Button from "@material-ui/core/Button";
import PostContentInput from "../components/PostContentInput";
import { Redirect } from "react-router";
import Tags from "../components/Tags";
import TextField from "@material-ui/core/TextField";
import { Typography } from "@material-ui/core";
import { useHistory } from "react-router-dom";

/* Styles */
const useStyles = makeStyles((theme: Theme) => ({
	item: {
		marginBottom: 12,
	},
	container: {
		marginTop: theme.spacing(2),
	},
	text: {
		marginBottom: "5px",
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

export default function EditPostInputBox(post: Post) {
	const classes = useStyles();
	const dispatch = useDispatch();
	const history = useHistory();
	const [currentTag, setCurrentTag] = useState<string>(""); // The current value of the tag text field
	const [state, setState] = React.useState<Post>(post);
	const [tags, setTags] = useState<string[]>(state.tags); // The array of already entered tags
	const [snackbarMessage, setSnackbarMessage] = useState("");
	const [isSnackbarOpen, setIsSnackbarOpen] = useState(false);
	const [isUpdateEnabled, setIsUpdateEnabled] = React.useState(
		state.content !== "" && state.title !== ""
	);

	const list = require('badwords-list');
	const regex = list.regex;

    const onTagChange = (e: ChangeEvent<HTMLInputElement>) => {
		setCurrentTag(e.currentTarget.value);
	};

	// Handlers for tag management
	const onTagDelete = (index: number) => {
		const newTags = tags.filter((tag, i) => i !== index);
		setTags(newTags);
		const newState = {
			...state,
			tags: newTags,
		};
		setState((prevState) => newState);
	};

	const onTagKeyPress = (e: KeyboardEvent) => {
		if (e.key === "Enter") {
			if (tags.includes(currentTag)) {
				// Do not allow duplicate tags
				setSnackbarMessage("Duplicate tags are not allowed.");
				setIsSnackbarOpen(true);
			} 
			else if (currentTag === "") {
				// Do not allow empty tags
				setSnackbarMessage("empty tags are not allowed.");
				setIsSnackbarOpen(true);
			}
			else if (tags.length === 5) {
				// Only allow 5 tags
				setSnackbarMessage("Cannot exceed 5 tags per post.");
				setIsSnackbarOpen(true);
			} 
			else if (regex.test(currentTag) === true) {
				regex.lastIndex = 0;
				setCurrentTag("");
				// No bad words
				setSnackbarMessage("No bad words.");
				setIsSnackbarOpen(true);
			}
			else {
				const newTags = tags.concat(currentTag);
				setCurrentTag("");
				setTags(newTags);
				const newState = {
					...state,
					tags: newTags,
				};
				setState((prevState) => newState);
			}
		}
	};

	const onSnackbarClose = (
		event: SyntheticEvent,
		reason?: SnackbarCloseReason
	) => {
		if (reason === "clickaway") {
			// Don't disable snackbar on clickaway to give users time to read the message
			return;
		}
		setCurrentTag("");
		setIsSnackbarOpen(false);
	};

	const onChange = (event: ChangeEvent<HTMLInputElement>) => {
		const { name, value } = event.target;
		const newState = {
			...state,
			[name]: value,
		};
		console.log(name, value);
		setIsUpdateEnabled(newState.content !== "" && newState.title !== "");
		setState((prevState) => newState);
	};
	const onCancelClick = () => {
		console.log("Cancel clicked");
		history.push("/Post/" + state.post_id);
	};

	const onEditClick = async () => {
		console.log("Update button clicked!", state);
		// TODO: validation
		dispatch(
			sendUpdate(
				state,
				() => (window.location.href = "/Post/" + state.post_id)
			)
		);
	};

	const loggedinUserID = useSelector(selectUserID);
	const isAdmin = useSelector(selectIsAdmin);
	const isChangeable =
		state === undefined || state.author_id === loggedinUserID || isAdmin;

	return (
		<div>
			{isChangeable ? (
				<div className={classes.container}>
					<Typography
						align="left"
						className={classes.text}
						variant="body1"
					>
						Post Title
					</Typography>
					<TextField
						fullWidth
						multiline
						rows={1}
						placeholder="New Title "
						onChange={onChange}
						variant="outlined"
						name="title"
						value={state.title}
						autoFocus
					/>

					<div className={classes.container}>
						<Typography
							align="left"
							className={classes.text}
							variant="body1"
						>
							Post Tags
						</Typography>
						<TextField
							className={classes.textField}
							fullWidth={true}
							label="Enter tag and hit 'enter'"
							onChange={onTagChange}
							onKeyPress={onTagKeyPress}
							size="small"
							name="tags"
							value={currentTag}
							variant="outlined"
						/>
						<Tags
							onDelete={onTagDelete}
							tags={tags}
							shouldSort={false}
						/>
					</div>
					<div className={classes.container}>
						<PostContentInput
							onContentChange={onChange}
							content={state.content}
							onAppendContent={(s) =>
								setState((prevState) => ({
									...prevState,
									content: state.content + s,
								}))
							}
						/>
					</div>

					<div className={classes.root}>
						<Button
							color="secondary"
							variant="contained"
							onClick={onEditClick}
							disabled={!isUpdateEnabled}
						>
							Update
						</Button>
						<Button
							color="secondary"
							variant="outlined"
							onClick={onCancelClick}
						>
							Cancel
						</Button>
					</div>
				</div>
			) : (
				!isChangeable && <Redirect to={"/ViewPost" + post.post_id} />
			)}

			<Snackbar
				open={isSnackbarOpen}
				autoHideDuration={6000}
				onClose={onSnackbarClose}
			>
				<Alert onClose={onSnackbarClose} severity="error">
					{snackbarMessage}
				</Alert>
			</Snackbar>
		</div>
	);
}
