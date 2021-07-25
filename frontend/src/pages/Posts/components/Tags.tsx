import React, { MouseEvent } from "react";
import { Theme, makeStyles, withStyles } from "@material-ui/core/styles";

import Chip from "@material-ui/core/Chip";
import { useHistory } from "react-router-dom";

type TagsProps = {
    onDelete?: (index: number) => void;
    shouldSort: boolean;
    shouldPreventDefault?: boolean;
    tags: string[];
};

const useStyles = makeStyles((theme: Theme) => ({
	chip: {
		margin: theme.spacing(0.5),
		maxWidth: 200,
		whiteSpace: "nowrap",
		//display: "block", 
		textOverflow: "ellipsis"
	},
	chipContainer: {
		display: "flex",
		flexDirection: "row",
		flexWrap: "wrap",
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
    var tags;
    if (props.shouldSort) {
        tags = [...props.tags];
        tags.sort();
    } else {
        tags = props.tags;
    }

    const onDelete = props.onDelete;
    const onTagClick = (event: MouseEvent, tag: string) => {
        if (props.shouldPreventDefault) {
            event.preventDefault();
        }
        history.push("/Tag/" + tag);
    };

    return (
        <div className={classes.chipContainer}>
            {tags.map((tag: string, index: number) => {
                return (
                    <StyledChip
                        className={classes.chip}
                        color="primary"
                        key={index}
                        label={tag}
                        onClick={(event) => onTagClick(event, tag)}
                        onDelete={
                            onDelete !== undefined
                                ? () => onDelete(index)
                                : undefined
                        }
                        variant="outlined"
                    />
                );
            })}
        </div>
    );
}
