import React, { ChangeEvent, useState } from "react";
import { Theme, makeStyles } from "@material-ui/core/styles";

import Button from "@material-ui/core/Button";
import Dialog from "@material-ui/core/Dialog";
import DialogActions from "@material-ui/core/DialogActions";
import DialogContent from "@material-ui/core/DialogContent";
import DialogContentText from "@material-ui/core/DialogContentText";
import DialogTitle from "@material-ui/core/DialogTitle";
import ImageIcon from "@material-ui/icons/Image";
import Link from "@material-ui/core/Link";
import Markdown from "../../../common/Markdown";
import Paper from "@material-ui/core/Paper";
import TextField from "@material-ui/core/TextField";
import { Typography } from "@material-ui/core";

/* Styles */
const useStyles = makeStyles((theme: Theme) => ({
	button: {
		margin: theme.spacing(1),
	},
	preview: {
		padding: theme.spacing(4),
		minHeight: 300,
		overflowY: "auto",
		wordWrap: "break-word",
	},
	previewContainer: {
		textAlign: "left",
	},
	text: {
		marginBottom: "5px",
	},
	textField: {
		width: "100%",
		marginBottom: 10,
	},
}));

interface PostContentInputProps {
	onContentChange: (event: ChangeEvent<HTMLInputElement>) => void;
	onAppendContent: (toAppend: string) => void;
	content: string;
}
function PostContentInput({
	onContentChange,
	content,
	onAppendContent,
}: PostContentInputProps) {
	const classes = useStyles();

	const [open, setOpen] = useState(false);
	const [imageTitle, setImageTitle] = useState("");
	const [imageLink, setImageLink] = useState("");
	const handleClickOpen = () => {
		setOpen(true);
	};

	const handleAddButtonClicked = () => {
		onAppendContent("![" + imageTitle + "](" + imageLink + ")");
		handleClose();
	};

	const handleClose = () => {
		setOpen(false);
	};

	return (
		<div>
			<Typography className={classes.text} variant="body1" align="left">
				Post Content
			</Typography>
			<Typography className={classes.text} variant="body2" align="left">
				This component now supports markdown language.{" "}
				<Link href={"https://commonmark.org/help/"} target="_blank">
					Learn more about Markdown
				</Link>{" "}
				.
			</Typography>

			<div className={classes.previewContainer}>
				<Button
					variant="outlined"
					color="primary"
					size="medium"
					className={classes.button}
					startIcon={<ImageIcon />}
					onClick={handleClickOpen}
				>
					Add Image
				</Button>
				<TextField
					className={classes.textField}
					fullWidth={true}
					label="Enter content"
					multiline
					onChange={onContentChange}
					required={false}
					rows={10}
					rowsMax={10}
					size="small"
					variant="outlined"
					value={content}
					name="content"
				/>
				<Typography
					className={classes.text}
					variant="body1"
					align="left"
				>
					Preview
				</Typography>
				<Paper className={classes.preview}>
					<Markdown md={content} />
				</Paper>
			</div>
			<Dialog
				open={open}
				onClose={handleClose}
				aria-labelledby="form-dialog-title"
			>
				<DialogTitle id="form-dialog-title">Add Image</DialogTitle>
				<DialogContent>
					<DialogContentText>
						To add an image, please paste the image link below.
					</DialogContentText>
					<TextField
						autoFocus
						margin="dense"
						id="name"
						label="Image Title"
						type="text"
						fullWidth
						value={imageTitle}
						onChange={(e) => setImageTitle(e.target.value)}
					/>
					<TextField
						margin="dense"
						id="name"
						label="Image Link"
						type="url"
						fullWidth
						value={imageLink}
						onChange={(e) => setImageLink(e.target.value)}
					/>
					{imageLink && imageTitle && (
						<img src={imageLink} alt={imageTitle}></img>
					)}
				</DialogContent>
				<DialogActions>
					<Button onClick={handleClose} color="primary">
						Cancel
					</Button>
					<Button
						onClick={handleAddButtonClicked}
						color="primary"
						disabled={imageLink === "" || imageTitle === ""}
					>
						Add
					</Button>
				</DialogActions>
			</Dialog>
		</div>
	);
}

export default PostContentInput;
