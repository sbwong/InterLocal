import { Theme, makeStyles } from "@material-ui/core/styles";

import { Button } from "@material-ui/core";
import React from "react";

const useStyles = makeStyles((theme: Theme) => ({
	container: {
		borderRadius: 24,
		fontSize: "12px",
		maxHeight: "45px",
	},
}));

interface PostTypeTagProps {
	content_type: string;
}

const PostTypeTag = ({ content_type }: PostTypeTagProps) => {
	const classes = useStyles();
	return (
		<Button
			className={classes.container}
			variant="contained"
			disabled={true}
			color="secondary"
		>
			{content_type === "qa" ? "Q&A" : "Note"}
		</Button>
	);
};

export default PostTypeTag;
