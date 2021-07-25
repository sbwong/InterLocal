import React, { useEffect, useState } from "react";
import { Theme, makeStyles, withStyles } from "@material-ui/core/styles";
import {
	fetchAllPostCount,
	fetchTopTags,
	selectTopTags,
	//selectPostCount,
} from "../../../slices/postSlice";
import { useDispatch, useSelector } from "react-redux";

import Chip from "@material-ui/core/Chip";
import CircularProgress from "@material-ui/core/CircularProgress";
import Grid from "@material-ui/core/Grid";
import { Typography } from "@material-ui/core";
import { useHistory } from "react-router-dom";

const TOP_TAGS_NUM = 7;

type TagsProps = {
	tags?: string[];
	shouldSort?: boolean;
};

const useStyles = makeStyles((theme: Theme) => ({
	chip: {
		margin: theme.spacing(0.5),
		fontSize: "12pt",
		borderRadius: 24,
		flexGrow: 1,
		maxWidth: "100%",
		textOverflow: "ellipsis",
	},
	chipContainer: {
		display: "flex",
		flexDirection: "row",
		flexWrap: "wrap",
		paddingBottom: "20px",
		width: "80%",
		marginRight: "auto",
	},
	title: {
		width: "100%",
	},
	loading: {
		visibility: "hidden",
	},
}));

const StyledChip = withStyles({
	root: {
		"&&:hover": {
			backgroundColor: "#13133E",
			color: "white",
		},
		"&&:focus": {
			backgroundColor: "#13133E",
			color: "white",
		},
	},
})(Chip);

export default function Tags(props: TagsProps) {
	const history = useHistory();
	const classes = useStyles();
	const dispatch = useDispatch();
	const topTags = useSelector(selectTopTags);
	const [isFetching, setIsFetching] = useState(true);
	const [postCount, setPostCount] = useState(0);
	//const postCount = useSelector(selectPostCount);
	const existsPosts = postCount !== 0;

	const onTagClick = (tag: string) => {
		history.push("/Tag/" + tag);
	};

	useEffect(() => {
		(async () => {
			await fetchAllPostCount().then((result) => {
				setPostCount(result);
			});
			//await (dispatch(fetchPostCount()));
			await dispatch(fetchTopTags(TOP_TAGS_NUM));
			setIsFetching(false);
		})();
	}, [dispatch]);

	return isFetching ? (
		<CircularProgress className={classes.loading} />
	) : existsPosts ? (
		<div className={classes.chipContainer}>
			<Typography className={classes.title} variant="h5">
				Top Tags
			</Typography>

			{topTags.map((tag: string, index: number) => {
				return (
					<StyledChip
						className={classes.chip}
						color="primary"
						key={index}
						label={tag}
						onClick={() => onTagClick(tag)}
						variant="outlined"
						clickable
					/>
				);
			})}
		</div>
	) : (
		<Grid container justify="center">
			<Typography>No posts yet.</Typography>
		</Grid>
	);
}
