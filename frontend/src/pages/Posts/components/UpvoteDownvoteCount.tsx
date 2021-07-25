import { Theme, makeStyles } from "@material-ui/core/styles";

import { Post } from "../../../slices/postSlice";
import React from "react";
import { Typography } from "@material-ui/core";

const useStyles = makeStyles((theme: Theme) => ({
	parent: {
		display: "flex",
		flexDirection: "row",
		justifyContent: "center",
		float: "left",
		textAlign: "center",
		marginRight: "5px",
		borderRadius: theme.spacing(1),
	},
	number: {
		fontSize: 15,
	},
}));

const UpvoteDownvoteCount = (props: Post) => {
	const classes = useStyles();

	return (
		<div className={classes.parent}>
			<Typography
				className={classes.number}
				variant="subtitle1"
				color="inherit"
			>
				<b>{props.up_count + props.down_count}</b> Votes
			</Typography>
		</div>
	);
};

export default UpvoteDownvoteCount;
