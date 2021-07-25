import Button from "@material-ui/core/Button";
import React, { ChangeEvent, useState } from "react";
import { Theme, makeStyles } from "@material-ui/core/styles";
import { Container, Typography } from "@material-ui/core";
import { useDispatch } from "react-redux";
import FormLabel from '@material-ui/core/FormLabel';
import FormControl from '@material-ui/core/FormControl';
import FormGroup from '@material-ui/core/FormGroup';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Checkbox from '@material-ui/core/Checkbox';
import Radio from "@material-ui/core/Radio";
import RadioGroup from "@material-ui/core/RadioGroup";
import {
    fetchPosts, 
    fetchPostCount,
    PostType
} from "../../../slices/postSlice";

const POSTS_PER_PAGE = 10;

const useStyles = makeStyles((theme: Theme) => ({
    sidebar: {
        backgroundColor: "white",
        width: "80%",
        marginLeft: "auto",
        marginRight: "auto",
        borderRadius: 12,
        display: "inline-block",
        flexDirection: "column",
        alignItems: "flex-start",
        alignContent: "center",
        padding: 20,
        textAlign: "left",
        flexWrap: "wrap",
        //height: "20%"
    },
    formControl: {
        //margin: theme.spacing(1),
        //marginLeft: "auto",
        //marginRight: "auto",
        //backgroundColor: "green"
    },
    formGroup: {
        flexWrap: "wrap",
        marginBottom: "20px",
        flexDirection: "row",
    },
    checkbox: {
        boxSizing: "border-box",
        flexWrap: "wrap",
        marginBottom: "-15px"
    },
    button: {
        width: "100%",
        marginLeft: "auto",
        marginRight: "auto",
        marginTop: "10px",
        marginBottom: "10px"

    },
    label: {
        color: "black",
        fontWeight: "bolder"

    },
    radioButton: {
        margin: theme.spacing(0.1),
    },
    title: {
        marginBottom: "10px",
    }
}));


export default function FilterBy() {
    const classes = useStyles();
    const dispatch = useDispatch();
    const [dateState, setDateState] = React.useState({
        day: false,
        week: false,
        month: false,
        year: false,
    });

    const [contentState, setContentState] = React.useState({
        note: false,
        qa: false
    })

    const [userState, setUserState] = React.useState({
        admin: false
    })

    const [tagsState, setTagsState] = React.useState({
        transportation: false,
        techSupport: false,
        immigration: false,
        academics: false,
        culture: false,
    });

    const [userType, setUserType] = useState<string>("");
    const [contentType, setContentType] = useState<string>("");
    const [dateType, setDateType] = useState<string>("");

    const onContentTypeChange = (e: ChangeEvent<HTMLInputElement>) => {
        setContentState({ ...contentState, [e.target.name]: e.target.checked });
        if (e.target.checked) {
            setContentType(e.target.value as PostType);
        } else {
            setContentType("");
        }
    };
    const onTagChange = (e: ChangeEvent<HTMLInputElement>) => {
        setTagsState({ ...tagsState, [e.target.name]: e.target.checked });
    };
    const onUserTypeChange = (e: ChangeEvent<HTMLInputElement>) => {
        setUserState({ ...userState, [e.target.name]: e.target.checked });
        if (e.target.checked) {
            setUserType(e.currentTarget.value);
        } else {
            setUserType("");
        }
        
    };
    const onDateChange = (e: ChangeEvent<HTMLInputElement>) => {
        setDateState({ ...dateState, [e.target.name]: e.target.checked });
        setDateType(e.currentTarget.value);
    };
    
    const onSubmitClick = async () => {
        var tags: string[] | undefined;
        tags = [];
        for (const [k, v] of Object.entries(tagsState)) {
            if (v === true) {
                var tag = k[0].toUpperCase() + k.substring(1);
                tags.push(tag);
            }
        }        

        if (tags.length === 0) {
            tags = undefined;
        }

        var user;
        if (userType === "") {
            user = undefined;
        } else {
            user = userType;
        }

        var date;
        if (dateType === "") {
            date = undefined;
        } else {
            date = dateType;
        }

        var content;
        if (contentType === "") {
            content = undefined;
        } else if (contentState.note && contentState.qa) {
            content = undefined;
        } else {
            content = contentType;
        }
         
        console.log(tags);
        console.log(userType);
        await dispatch(fetchPostCount(undefined, tags, user, content, date));
        await dispatch(fetchPosts(POSTS_PER_PAGE, 0, undefined, tags, true, user, content, date));
        
    };

    const onResetClick = async () => {
        setUserType("");
        setUserState({
            admin: false,
        });
        setDateType("");
        setDateState({
            day: false,
            week: false,
            month: false,
            year: false
        });
        setContentType("");
        setContentState({
            qa: false,
            note: false
        });
        setTagsState({
            transportation: false,
            techSupport: false,
            immigration: false,
            academics: false,
            culture: false,
        });
        //setContentType(undefined);
        await dispatch(fetchPosts(POSTS_PER_PAGE, 0, undefined, undefined, true));
    };

    const { qa, note } = contentState;
    const { admin } = userState;
    //const { day, week, month, year } = dateState;
    const { transportation, techSupport, immigration, academics, culture } = tagsState;

    return (
        <Container className={classes.sidebar}>
            <Typography className={classes.title} variant="h5">Filter Posts By</Typography>
            {
                <FormControl component="fieldset" className={classes.formControl}>
                    <FormLabel component="legend" className={classes.label}>Date</FormLabel>
                    <FormGroup className={classes.formGroup}>
                        <RadioGroup
                            aria-label="type"
                            name="type"
                            onChange={onDateChange}
                            row
                        >
                            <FormControlLabel
                                className={classes.radioButton}
                                checked={dateType === "day"}
                                control={<Radio color="secondary" />}
                                label="Day"
                                labelPlacement="end"
                                value="day"
                            />
                            <FormControlLabel
                                className={classes.radioButton}
                                checked={dateType === "week"}
                                control={<Radio color="secondary" />}
                                label="Week"
                                labelPlacement="end"
                                value="week"
                            />
                            <FormControlLabel
                                className={classes.radioButton}
                                checked={dateType === "month"}
                                control={<Radio color="secondary" />}
                                label="Month"
                                labelPlacement="end"
                                value="month"
                            />
                            <FormControlLabel
                                className={classes.radioButton}
                                checked={dateType === "year"}
                                control={<Radio color="secondary" />}
                                label="Year"
                                labelPlacement="end"
                                value="year"
                            />
                        </RadioGroup>
                    </FormGroup>
                    <FormLabel component="legend" className={classes.label}>Author</FormLabel>
                    <FormGroup className={classes.formGroup}>
                        <FormControlLabel
                            control={<Checkbox checked={admin} onChange={onUserTypeChange} name="admin" />}
                            label="Admin"
                            value="admin"
                            className={classes.checkbox}
                        />
                    </FormGroup>
                    <FormLabel component="legend" className={classes.label}> Post Type</FormLabel>
                    <FormGroup className={classes.formGroup}>
                        <FormControlLabel
                            control={<Checkbox checked={note} onChange={onContentTypeChange} name="note" />}
                            label="Note"
                            value="note"
                            className={classes.checkbox}
                        />
                        <FormControlLabel
                            control={<Checkbox checked={qa} onChange={onContentTypeChange} name="qa" />}
                            label="Q&A"
                            value="qa"
                            className={classes.checkbox}
                        />
                    </FormGroup>
                    <FormLabel component="legend" className={classes.label}>Tags</FormLabel>
                    <FormGroup className={classes.formGroup}>
                        <FormControlLabel
                            control={<Checkbox checked={immigration} onChange={onTagChange} name="immigration" />}
                            label="Immigration"
                            className={classes.checkbox}
                        />
                        <FormControlLabel
                            control={<Checkbox checked={culture} onChange={onTagChange} name="culture" />}
                            label="Culture"
                            className={classes.checkbox}
                        />
                        <FormControlLabel
                            control={<Checkbox checked={academics} onChange={onTagChange} name="academics"/>}
                            label="Academics"
                            className={classes.checkbox}
                        />
                        <FormControlLabel
                            control={<Checkbox checked={transportation} onChange={onTagChange} name="transportation" />}
                            label="Transportation"
                            className={classes.checkbox}
                        />
                        <FormControlLabel
                            control={<Checkbox checked={techSupport} onChange={onTagChange} name="techSupport" />}
                            label="Tech Support"
                            className={classes.checkbox}
                        />
                    </FormGroup>
                </FormControl>
            }
            <Button
                color="secondary"
                className={classes.button}
                onClick={onSubmitClick}
                variant="contained"
            >
                Submit
			</Button>
            <Button
                color="secondary"
                className={classes.button}
                onClick={onResetClick}
                variant="contained"
            >
                Reset
			</Button>
        </Container>
    );
}