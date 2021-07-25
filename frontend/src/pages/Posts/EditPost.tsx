import { Post, fetchPost } from "../../slices/postSlice";
import React, { useEffect, useState } from "react";
import { Theme, createStyles, makeStyles } from "@material-ui/core/styles";

import Alert from "@material-ui/lab/Alert";
import ArrowBackIcon from "@material-ui/icons/ArrowBack";
import Box from "@material-ui/core/Box";
import Chip from "@material-ui/core/Chip";
import CircularProgress from "@material-ui/core/CircularProgress";
import Container from "@material-ui/core/Container";
import EditPostInputBox from "./components/EditPostInputBox";
import IconButton from "@material-ui/core/IconButton";
import Markdown from "../../common/Markdown";
import PostTypeTag from "./components/PostTypeTag";
import PostedByLabel from "../../common/PostedByLabel";
import Typography from "@material-ui/core/Typography";
import { useHistory } from "react-router-dom";

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
		},
		header: {
			height: "25%",
			alignItems: "left",
			justifyContent: "left",
			display: "flex",
			flexDirection: "row",
			padding: 20,
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
			padding: 20,
		},
		root: {
			"& > *": {
				margin: theme.spacing(2),
			},
		},
		tagButton: {
			borderRadius: 24,
			marginRight: 12,
		},
		textField: {
			width: "80%",
			marginBottom: 10,
		},
	})
);

export function UpdatePost(props: any) {
	const classes = useStyles();
	const history = useHistory();

	const [currentPost, setCurrentPost] = useState<Post>();
	const [doctitle, setDocTitle] = useState("Post Title Here");
	const [isFetching, setIsFetching] = useState(true); // Show loading screen if fetching.
	const [isError, setIsError] = useState(false); // Show error screen if fetching post fails.
	const postID = props?.match.params.post_id;

	useEffect(() => {
		fetchPost(postID).then((result) => {
			if (result !== undefined) {
				setCurrentPost(result);
				setDocTitle(result.title);
				setIsFetching(false);
			} else {
				// API could not fetch post based on provided post ID
				console.error("Unable to fetch post");
				setIsError(true);
			}
		});
	}, [postID]);

	if (isFetching) {
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
						aria-label="back to original post"
						onClick={() => {
							history.goBack();
						}}
						style={{ maxHeight: "50px", maxWidth: "50px" }}
					>
						<ArrowBackIcon />
					</IconButton>

					<Container className={classes.container}>
						<Typography className={classes.item} variant="h4">
							{currentPost.title}
						</Typography>
						<PostedByLabel
							username={currentPost.username}
							author_id={currentPost.author_id}
							created_time={currentPost.created_time}
							last_edit_time={currentPost.last_edit_time}
							is_comment={false}
							is_admin={currentPost.is_admin}
						/>
						<div className={classes.chipContainer}>
							{currentPost.tags.map(
								(tag: string, index: number) => {
									return (
										<Chip
											className={classes.chip}
											color="primary"
											key={index}
											label={tag}
											variant="outlined"
										/>
									);
								}
							)}
						</div>
					</Container>
					<PostTypeTag content_type={currentPost.content_type} />
				</Container>

				<Container className={classes.container}>
					<Markdown md={currentPost.content} />
				</Container>

				<Container className={classes.container}>
					<Box
						marginTop="50px"
						marginBottom="100px"
						justifyContent="center"
					>
						<EditPostInputBox {...currentPost} />
					</Box>
				</Container>
			</div>
		);
	}
}
