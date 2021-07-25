import { Container, Typography } from "@material-ui/core";
import { Theme, makeStyles } from "@material-ui/core/styles";

import Card from "@material-ui/core/Card";
import CardActionArea from "@material-ui/core/CardActionArea";
import Markdown from "../../../common/Markdown";
import { POST_CLICKED } from "../../../constants/Metrics";
import { Post } from "../../../slices/postSlice";
import PostTypeTag from "./PostTypeTag";
import PostedByLabel from "../../../common/PostedByLabel";
import React from "react";
import Tags from "./Tags";
import UpvoteDownvoteCount from "./UpvoteDownvoteCount";
import { logIncrementCustomMetric } from "../../../adapters/logging";

const useStyles = makeStyles((theme: Theme) => ({
	card: {
		backgroundColor: "white",
		borderRadius: 12,
		display: "flex",
		flexDirection: "row",
		marginBottom: theme.spacing(1),
		marginTop: theme.spacing(1),
		textAlign: "left",
		textDecoration: "none",
		width: "80%",
		overflow: "auto",
		justifyContent: "space-between",
	},
	container: {
		padding: theme.spacing(4),
	},
	// Content body truncates with ellipses after 10 lines of text.
	// https://stackoverflow.com/questions/7993067/text-overflow-ellipsis-not-working
	contentDisplay: {
		margin: 0,
		marginTop: 4,
		padding: 0,
		overflow: "hidden",
		textOverflow: "ellipsis",
		display:
			"-webkit-box;-webkit-line-clamp: 3; -webkit-box-orient: vertical;",
	},
	postInfo: {
		whiteSpace: "nowrap",
		textOverflow: "ellipsis",
		maxWidth: "100%",
		display: "block",
		overflow: "hidden",
		paddingRight: 50,
	},
	columnContainerLeft: {
		display: "flex",
		flexDirection: "column",
		textOverflow: "ellipsis",
		overflow: "hidden",
	},
	columnContainerRight: {
		display: "flex",
		flexDirection: "column",
		alignItems: "flex-end",
		flexShrink: 0,
		flexGrow: 0,
	},
	top: {
		display: "flex",
		flexDirection: "row",
		justifyContent: "space-between",

		overflow: "hidden",
	},
}));

const PostPreviewBox = (props: Post) => {
	const classes = useStyles();
	const onPostClick = () => {
		logIncrementCustomMetric(
			POST_CLICKED,
			`Post ${props.post_id} was clicked!`
		);
	};
	return (
		<Card className={classes.card}>
			<CardActionArea
				href={"/Post/" + props.post_id}
				color="inherit"
				onClick={onPostClick}
			>
				<Container className={classes.container}>
					<div className={classes.top}>
						<div className={classes.columnContainerLeft}>
							<Typography
								noWrap
								className={classes.postInfo}
								variant="h4"
							>
								{props.title}
							</Typography>
							<PostedByLabel {...props} is_comment={false} />
						</div>

						<div className={classes.columnContainerRight}>
							<PostTypeTag content_type={props.content_type} />
							<UpvoteDownvoteCount {...props} />
						</div>
					</div>

					<Container className={classes.contentDisplay}>
						<Markdown md={props.content} />
					</Container>
					<Tags
						shouldSort={true}
						shouldPreventDefault={true}
						tags={props.tags}
					/>
				</Container>
			</CardActionArea>
		</Card>
	);
};

export default PostPreviewBox;
