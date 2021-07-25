import { PostRequest, PostType, sendCreate } from "../../slices/postSlice";
import React, {
    ChangeEvent,
    KeyboardEvent,
    SyntheticEvent,
    useState,
} from "react";
import Snackbar, { SnackbarCloseReason } from "@material-ui/core/Snackbar";
import { Theme, makeStyles } from "@material-ui/core/styles";

import Alert from "@material-ui/lab/Alert";
import Button from "@material-ui/core/Button";
import Card from "@material-ui/core/Card";
import Container from "@material-ui/core/Container";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import PostContentInput from "./components/PostContentInput";
import Radio from "@material-ui/core/Radio";
import RadioGroup from "@material-ui/core/RadioGroup";
import Tags from "./components/Tags";
import TextField from "@material-ui/core/TextField";
import { Typography } from "@material-ui/core";
import { useDispatch } from "react-redux";
import { useHistory } from "react-router-dom";

const MAX_TITLE_CHAR = 150;
const MAX_TAG_CHAR = 15;

/* Styles */
const useStyles = makeStyles((theme: Theme) => ({
    buttonContainer: {
        "& > *": {
            margin: theme.spacing(2),
        },
    },
    card: {
        borderStyle: "dashed",
        borderWidth: "2px",
        display: "flex",
        flexDirection: "row",
        justifyContent: "left",
        marginBottom: theme.spacing(2),
    },
    container: {
        alignItems: "left",
        display: "flex",
        flexDirection: "column",
        marginTop: theme.spacing(4),
    },
    label: {
        marginLeft: theme.spacing(2),
    },
    root: {
        backgroundColor: "#F39327",
    },
    row: {
        display: "flex",
        flexDirection: "row",
        justifyContent: "space-between",
        width: "100%",
    },
    tagContainer: {
        width: "60%",
    },
    text: {
        marginBottom: "5px",
    },
    textField: {
        width: "100%",
        marginBottom: 10,
    },
    typeContainer: {
        width: "25%",
        alignItems: "left",
    },
}));

export default function CreatePost() {
    const classes = useStyles();
    const dispatch = useDispatch();
    const history = useHistory();

    const list = require("badwords-list");
    const regex = list.regex;

    const [currentTag, setCurrentTag] = useState<string>(""); // The current value of the tag text field
    const [content, setContent] = useState("");
    const [isSnackbarOpen, setIsSnackbarOpen] = useState(false);
    const [tags, setTags] = useState<string[]>([]); // The array of already entered tags
    const [tagCharCount, setTagCharCount] = useState<number>(0);
    const [title, setTitle] = useState("");
    const [postType, setPostType] = useState<PostType>();
    const [titleCharCount, setTitleCharCount] = useState<number>(0);
    const [snackbarMessage, setSnackbarMessage] = useState("");

    // Handlers for text field changes
    const onContentChange = (event: ChangeEvent<HTMLInputElement>) => {
        setContent(event.target.value);
    };
    const onRadioChange = (event: ChangeEvent<HTMLInputElement>) => {
        setPostType(event.target.value as PostType);
    };
    const onTagChange = (e: ChangeEvent<HTMLInputElement>) => {
        setCurrentTag(e.currentTarget.value);
        setTagCharCount(e.currentTarget.value.length);
    };
    const onTitleChange = (event: ChangeEvent<HTMLInputElement>) => {
        setTitle(event.target.value);
        setTitleCharCount(event.target.value.length);
    };

    const onPostClick = async () => {
        if (postType == null) {
            console.log(postType);
            console.error(
                "Control should not reach here since button should be disabled until postType is selected."
            );
            return;
        }
        const post: PostRequest = {
            content: content,
            post_type: postType,
            tags: tags,
            title: title,
        };
        await dispatch(
            sendCreate(post, () => (window.location.href = "/Home/"))
        );
    };

    // Handlers for tag management
    const onTagDelete = (index: number) => {
        const newTags = tags.filter((tag, i) => i !== index);
        setTags(newTags);
    };
    const onTagKeyPress = (e: KeyboardEvent) => {
        if (e.key === "Enter") {
            if (tags.includes(currentTag)) {
                // Do not allow duplicate tags
                setSnackbarMessage("Duplicate tags are not allowed.");
                setIsSnackbarOpen(true);
            } else if (currentTag === "") {
                // Do not allow empty tags
                setSnackbarMessage("empty tags are not allowed.");
                setIsSnackbarOpen(true);
            } else if (tags.length === 5) {
                // Only allow 5 tags
                setSnackbarMessage("Cannot exceed 5 tags per post.");
                setIsSnackbarOpen(true);
            } else if (regex.test(currentTag) === true) {
                regex.lastIndex = 0;
                setCurrentTag("");
                // No bad words
                setSnackbarMessage("No bad words.");
                setIsSnackbarOpen(true);
            } else {
                const newTags = tags.concat(currentTag);
                setCurrentTag("");
                setTags(newTags);
            }
        }
    };
    const onSnackbarClose = (
        event: SyntheticEvent,
        reason?: SnackbarCloseReason
    ) => {
        if (reason === "clickaway") {
            // Don't disable snackbar on clickaway to give users time to read the message
            return;
        }
        setCurrentTag("");
        setIsSnackbarOpen(false);
    };

    // Only enable post button when all required fields are filled out
    const isPostable =
        title !== "" && postType !== undefined && postType !== null;

    return (
        <Container className={classes.container}>
            <div className={classes.typeContainer}>
                <Typography
                    className={classes.text}
                    variant="body1"
                    align="left"
                >
                    Post Type
                </Typography>
                <Card className={classes.card} variant="outlined">
                    <RadioGroup
                        aria-label="type"
                        name="type"
                        onChange={onRadioChange}
                        row
                    >
                        <FormControlLabel
                            className={classes.label}
                            control={<Radio color="primary" />}
                            label={PostType.Note}
                            labelPlacement="end"
                            value={PostType.Note}
                        />
                        <FormControlLabel
                            className={classes.label}
                            control={<Radio color="primary" />}
                            label={PostType.Question}
                            labelPlacement="end"
                            value={PostType.Question}
                        />
                    </RadioGroup>
                </Card>
            </div>
            <Typography className={classes.text} variant="body1" align="left">
                Post Title
            </Typography>
            <TextField
                className={classes.textField}
                fullWidth={true}
                label="Enter title"
                onChange={onTitleChange}
                required={true}
                size="small"
                variant="outlined"
                inputProps={{ maxLength: MAX_TITLE_CHAR }}
                helperText={titleCharCount + " / " + MAX_TITLE_CHAR}
            />

            <div className={classes.row}>
                <div className={classes.tagContainer}>
                    <Typography
                        align="left"
                        className={classes.text}
                        variant="body1"
                    >
                        Post Tags
                    </Typography>
                    <TextField
                        className={classes.textField}
                        fullWidth={true}
                        label="Enter tag and hit 'enter'"
                        onChange={onTagChange}
                        onKeyPress={onTagKeyPress}
                        size="small"
                        value={currentTag}
                        inputProps={{ maxLength: MAX_TAG_CHAR }}
                        helperText={tagCharCount + " / " + MAX_TAG_CHAR}
                        variant="outlined"
                    />
                </div>
            </div>
            <Tags onDelete={onTagDelete} tags={tags} shouldSort={false} />
            <PostContentInput
                onContentChange={onContentChange}
                content={content}
                onAppendContent={(s) => setContent(content + s)}
            />
            <div className={classes.buttonContainer}>
                <Button
                    color="secondary"
                    disabled={!isPostable}
                    onClick={onPostClick}
                    variant="contained"
                    classes={{ root: classes.root }}
                >
                    Post
                </Button>
                <Button
                    color="secondary"
                    onClick={() => {
                        history.goBack();
                    }}
                    variant="outlined"
                >
                    Cancel
                </Button>
            </div>
            <Snackbar
                open={isSnackbarOpen}
                autoHideDuration={6000}
                onClose={onSnackbarClose}
            >
                <Alert onClose={onSnackbarClose} severity="error">
                    {snackbarMessage}
                </Alert>
            </Snackbar>
        </Container>
    );
}
