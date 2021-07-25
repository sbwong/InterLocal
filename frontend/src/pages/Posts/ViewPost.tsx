import {
	Post,
	fetchComments,
	fetchPost,
	sendDeletion,
} from "../../slices/postSlice";
import React, { SyntheticEvent, useEffect, useState } from "react";
import Snackbar, { SnackbarCloseReason } from "@material-ui/core/Snackbar";
import { Theme, createStyles, makeStyles } from "@material-ui/core/styles";
import { selectIsAdmin, selectUserID } from "../../slices/profileSlice";
import { useDispatch, useSelector } from "react-redux";

import Alert from "@material-ui/lab/Alert";
import ArrowBackIcon from "@material-ui/icons/ArrowBack";
import Box from "@material-ui/core/Box";
import Button from "@material-ui/core/Button";
import CircularProgress from "@material-ui/core/CircularProgress";
import CloseIcon from "@material-ui/icons/Close";
import { Comment } from "../../slices/postSlice";
import CommentBox from "./components/commentBox";
import CommentInputBox from "./components/CommentInputBox";
import Container from "@material-ui/core/Container";
import DeleteDialog from "./components/DeleteDialog";
import FormControl from "@material-ui/core/FormControl";
import IconButton from "@material-ui/core/IconButton";
import InputLabel from "@material-ui/core/InputLabel";
import Link from "@material-ui/core/Link";
import Markdown from "../../common/Markdown";
import MenuItem from "@material-ui/core/MenuItem";
import Paper from "@material-ui/core/Paper";
import PostTypeTag from "./components/PostTypeTag";
import PostedByLabel from "../../common/PostedByLabel";
import Select from "@material-ui/core/Select";
import Tags from "./components/Tags";
import Typography from "@material-ui/core/Typography";
import UpvoteDownvote from "./components/UpvoteDownvote";
import { useCallback } from "react";
import { useHistory } from "react-router-dom";
import { utcToZonedTime } from "date-fns-tz";

const useStyles = makeStyles((theme: Theme) =>
	createStyles({
		body: {
			alignItems: "left",
			justifyContent: "left",
			display: "flex",
			flexDirection: "row",
			padding: 20,
		},
		chip: {
			margin: theme.spacing(0.5),
		},
		chipContainer: {
			display: "flex",
			flexDirection: "row",
			flexWrap: "wrap",
		},
		container: {
			alignItems: "left",
			justifyContent: "flex-start",
			display: "flex",
			flexDirection: "column",
			textAlign: "left",
			marginVertical: "10px",
			whiteSpace: "pre-line",
		},
		formControl: {
			margin: theme.spacing(1),
			minWidth: 200,
		},
		header: {
			height: "25%",
			alignItems: "left",
			justifyContent: "left",
			display: "flex",
			flexDirection: "row",
			width: "90%",
		},
		item: {
			marginBottom: 12,
		},
		loading: {
			marginTop: "20px",
		},
		postDetails: {
			display: "flex",
			flexDirection: "row",
			textAlign: "left",
			textDecoration: "none",
			justifyContent: "space-between",
		},
		page: {
			alignItems: "left",
			textAlign: "left",
			justifyContent: "left",
			display: "flex",
			overflow: "auto",
			width: "100%",
			flexDirection: "column",
			paddingTop: 20,
		},
		preview: {
			padding: theme.spacing(4),
			overflowY: "auto",
			wordWrap: "break-word",
			width: "95%",
		},
		root: {
			"& > *": {
				margin: theme.spacing(2),
			},
			display: "flex",
			flexDirection: "row",
		},
		subInfoContainer: {
			display: "flex",
			flexDirection: "row",
			alignItems: "flex-start",
			justifyContent: "flex-start",
		},
		tagButton: {
			borderRadius: 24,
			paddingRight: 40,
		},
		textField: {
			width: "80%",
			marginBottom: 10,
		},
		textBlock: {
			display: "flex",
			flexDirection: "row",
			alignItems: "center",
			justifyContent: "flex-start",
		},
	})
);

export function ViewPost(props: any) {
	const classes = useStyles();
	const dispatch = useDispatch();
	const history = useHistory();
	const [comments, setComments] = useState<Comment[]>([]);
	const [currentPost, setCurrentPost] = useState<Post>();
	const [doctitle, setDocTitle] = useState("Post Title Here");
	const [isDeleting, setIsDeleting] = useState(false); // Show loading screen if deleting.
	const [isFetching, setIsFetching] = useState(true); // Show loading screen if fetching.
	const [isError, setIsError] = useState(false); // Show error screen if fetching post fails.
	const [isSnackbarOpen, setIsSnackbarOpen] = useState(false);
	const [snackbarMessage, setSnackbarMessage] = useState("");
	const [commentFilter, setCommentFilter] = useState(1);
	const postID = props?.match.params.post_id;
	const loggedinUserID = useSelector(selectUserID);
	const editURL = "/EditPost/" + postID;

	const navigateToHome = () => {
		history.push("/Home");
	};

	const onDeleteClick = async () => {
		if (currentPost == null) {
			console.error(
				"Could not delete post because a post has not loaded yet."
			);
			return;
		}
		setIsDeleting(true);
		await dispatch(sendDeletion(currentPost, navigateToHome));
	};

	const onSnackbarClose = (
		event: SyntheticEvent,
		reason?: SnackbarCloseReason
	) => {
		if (reason === "clickaway") {
			// Don't disable snackbar on clickaway to give users time to read the message
			return;
		}
		setIsSnackbarOpen(false);
	};

	const handleChangeCommentFilter = (
		event: React.ChangeEvent<{ value: unknown }>
	) => {
		setCommentFilter(event.target.value as number);
	};

	const commentSection = (
		<Container className={classes.container}>
			<Box marginTop="50px" marginBottom="100px" justifyContent="center">
				<CommentInputBox
					postID={postID}
					createCommentHandler={(newComment) => {
						if (newComment) {
							setComments([newComment, ...comments]);
						} else {
							setSnackbarMessage(
								"You are logged out. Please log in to add a comment."
							);
							setIsSnackbarOpen(true);
						}
					}}
				/>
				<Typography variant="h5">Comments</Typography>
				{comments.length > 0 && (
					<div>
						<FormControl className={classes.formControl}>
							<InputLabel>Sort Comments By</InputLabel>
							<Select
								value={commentFilter}
								onChange={handleChangeCommentFilter}
							>
								<MenuItem value={1}>Upvotes</MenuItem>
								<MenuItem value={2}>Time</MenuItem>
							</Select>
						</FormControl>
					</div>
				)}
				{comments?.map((c) => {
					return (
						<CommentBox
							key={c.comment_id}
							comment={c}
							errorHandler={() => {
								setSnackbarMessage(
									"You are logged out. Please log in to upvote or downvote."
								);
								setIsSnackbarOpen(true);
							}}
						/>
					);
				})}
			</Box>
		</Container>
	);

	// Compare two comment timestamps (they're originally strings).
	const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
	const commentDateComparator = useCallback(
		(com1: Comment, com2: Comment) => {
			const d1 = utcToZonedTime(com1.created_time + "Z", timeZone);
			const d2 = utcToZonedTime(com2.created_time + "Z", timeZone);
			return +d2 - +d1;
		},
		[timeZone]
	);

	// "Impressions" are the sum of upvotes and downvotes for a given comment
	// and are a proxy for comment popularity.
	const commentImpressionComparator = (com1: Comment, com2: Comment) => {
		const com1Impressions = com1.up_count - com1.down_count;
		const com2Impressions = com2.up_count - com2.down_count;
		if (com1Impressions > com2Impressions) {
			return -1;
		} else if (com1Impressions < com2Impressions) {
			return 1;
		}
		return 0;
	};

	// Extensible for future Comment Filters!
	const commentComparator = useCallback(
		(com1: Comment, com2: Comment) => {
			switch (commentFilter) {
				case 2:
					return commentDateComparator(com1, com2);
				default:
					return commentImpressionComparator(com1, com2);
			}
		},
		[commentDateComparator, commentFilter]
	);

	// NOTES: useEffect to set the current post based on passed in post_id.
	// TODO: Should we convert this to an inner async function like the useEffect below to get the comments?
	// Is it okay to keep the "fetchPost" method attached to the profileSlice?
	useEffect(() => {
		fetchPost(postID).then((result) => {
			if (result !== undefined) {
				setCurrentPost(result);
				fetchComments(postID).then((comments) => {
					setComments(
						comments.sort((com1, com2) =>
							commentComparator(com1, com2)
						)
					);
				});
				setDocTitle(result.title);
				setIsFetching(false);
			} else {
				// API could not fetch post based on provided post ID
				console.error("Unable to fetch post");
				setIsError(true);
			}
		});
	}, [postID, commentFilter, commentComparator]);

	const isAdmin = useSelector(selectIsAdmin);
	const isChangeable =
		currentPost === undefined ||
		currentPost.author_id === loggedinUserID ||
		isAdmin;

	if (isFetching || isDeleting) {
		return <CircularProgress className={classes.loading} />;
	} else if (isError || currentPost === undefined) {
		return (
			<Alert severity="error">
				The post you are looking for is currently unavailable. Please
				try again.
			</Alert>
		);
	} else {
		document.title = doctitle;
		return (
			<div className={classes.page}>
				<Container className={classes.header}>
					<IconButton
						edge="start"
						color="inherit"
						aria-label="back to home"
						href="/Home"
						style={{ height: "20px", width: "20px" }}
					>
						<ArrowBackIcon />
					</IconButton>
					<Container className={classes.container}>
						<Typography
							style={{ wordWrap: "break-word" }}
							className={classes.item}
							variant="h3"
						>
							{currentPost.title}
						</Typography>
						<div className={classes.subInfoContainer}>
							<div>
								<PostedByLabel
									author_id={currentPost.author_id}
									username={currentPost.username}
									created_time={currentPost.created_time}
									last_edit_time={currentPost.last_edit_time}
									is_comment={false}
									is_admin={currentPost.is_admin}
								/>
								<Tags
									tags={currentPost.tags}
									shouldSort={true}
								/>
							</div>
						</div>
					</Container>
					<PostTypeTag content_type={currentPost.content_type} />
				</Container>

				<Container className={classes.container}>
					<div className={classes.textBlock}>
						<div>
							<UpvoteDownvote
								post={currentPost}
								errorHandler={() => {
									setSnackbarMessage(
										"You are logged out. Please log in to upvote or downvote."
									);
									setIsSnackbarOpen(true);
								}}
							/>
						</div>
						<Paper className={classes.preview}>
							<Markdown md={currentPost.content} />
						</Paper>
					</div>

					{isChangeable && (
						<div className={classes.root}>
							<Link href={editURL}>
								<Button color="primary" variant="contained">
									Update
								</Button>
							</Link>

							<DeleteDialog
								successCallback={onDeleteClick}
								isIcon={false}
							/>
						</div>
					)}
				</Container>

				{commentSection}
				<Snackbar open={isSnackbarOpen} onClose={onSnackbarClose}>
					<Alert
						onClose={onSnackbarClose}
						severity={"error"}
						action={
							<React.Fragment>
								<Button
									color="inherit"
									size="small"
									onClick={(e) => history.push("/Login")}
								>
									LOGIN
								</Button>
								<IconButton
									size="small"
									aria-label="close"
									color="inherit"
									onClick={onSnackbarClose}
								>
									<CloseIcon fontSize="small" />
								</IconButton>
							</React.Fragment>
						}
					>
						{snackbarMessage}
					</Alert>
				</Snackbar>
			</div>
		);
	}
}
