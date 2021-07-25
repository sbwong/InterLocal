import { Container, Typography } from "@material-ui/core";
import { Theme, makeStyles } from "@material-ui/core/styles";

import Card from "@material-ui/core/Card";
import CardActionArea from "@material-ui/core/CardActionArea";
import { POST_CLICKED } from "../../../constants/Metrics";
import { Post } from "../../../slices/postSlice";
import PostTypeTag from "./PostTypeTag";
import PostedByLabel from "../../../common/PostedByLabel";
import React from "react";
import Tags from "../components/Tags";
import { logIncrementCustomMetric } from "../../../adapters/logging";

const useStyles = makeStyles((theme: Theme) => ({
	card: {
		backgroundColor: "white",
		borderRadius: 12,
		width: "100%",
		display: "flex",
		flexDirection: "row",
		//padding: 10,
		paddingLeft: 0,
		marginTop: "10px",
		marginBottom: "10px",
		marginLeft: "auto",
		marginRight: "auto",
		textAlign: "left",
		textDecoration: "none",
		justifyContent: "space-evenly",
	},
	container: {
		padding: 10,
	},
	postInfo: {
		alignItems: "flex-start",
		alignContent: "flex-start",
		display: "flex",
		flexDirection: "column",
		flexWrap: "wrap",
	},
	// Content body truncates with ellipses after 10 lines of text.
	// https://stackoverflow.com/questions/7993067/text-overflow-ellipsis-not-working
	contentDisplay: {
		marginLeft: "10px",
		margin: 0,
		marginTop: 4,
		padding: 0,
		overflow: "hidden",
		textOverflow: "ellipsis",
		display:
			"-webkit-box;-webkit-line-clamp: 1; -webkit-box-orient: vertical;",
	},
	title: {
		fontSize: "14pt",
		overflow: "hidden",
		textOverflow: "ellipsis",
		display:
			"-webkit-box;-webkit-line-clamp: 10; -webkit-box-orient: vertical;",
	},
}));

const RecentPostPreviewBox = (props: Post) => {
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
					<Container className={classes.contentDisplay}>
						<Typography className={classes.title} variant="h5">
							{props.title}
						</Typography>
						<PostedByLabel
							username={props.username}
							author_id={props.author_id}
							created_time={props.created_time}
							last_edit_time={props.last_edit_time}
							is_comment={false}
							is_admin={props.is_admin}
						/>
						<Tags
							tags={props.tags}
							shouldPreventDefault={true}
							shouldSort={true}
						/>
					</Container>
					<div>
						<PostTypeTag content_type={props.content_type} />
					</div>
				</Container>
			</CardActionArea>
		</Card>
	);
};

export default RecentPostPreviewBox;
