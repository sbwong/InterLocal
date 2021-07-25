import { Theme, createStyles, makeStyles } from "@material-ui/core/styles";

import AddIcon from "@material-ui/icons/Add";
import Fab from "@material-ui/core/Fab";
import Link from "@material-ui/core/Link";
import React from "react";

const useStyles = makeStyles((theme: Theme) =>
	createStyles({
		button: {
			margin: 0,
			top: "auto",
			right: theme.spacing(8),
			bottom: theme.spacing(8),
			left: "auto",
			position: "fixed",
			zIndex: 1
		},
		extendedIcon: {
			marginRight: theme.spacing(1),
		},
	})
);

export default function CreatePostFAB() {
	const classes = useStyles();

	return (
		<Link href="/CreatePost" underline={"none"}>
			<Fab
				variant="extended"
				color="secondary"
				aria-label="add"
				className={classes.button}
			>
				<AddIcon className={classes.extendedIcon} />
				Create Post
			</Fab>{" "}
		</Link>
	);
}
