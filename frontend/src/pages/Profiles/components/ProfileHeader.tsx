import { Container, Typography } from "@material-ui/core";
import {
	ProfileState,
	fetchProfile,
	selectUserID,
} from "../../../slices/profileSlice";
import React, { useEffect, useState } from "react";
import { Theme, makeStyles } from "@material-ui/core/styles";
import { catchHandler, getInitials } from "../../../slices/profileSlice";
import { useDispatch, useSelector } from "react-redux";

import Alert from "@material-ui/lab/Alert";
import AvatarIcon from "../../../common/AvatarIcon";
import Box from "@material-ui/core/Box";
import Card from "@material-ui/core/Card";
import CardContent from "@material-ui/core/CardContent";
import CircularProgress from "@material-ui/core/CircularProgress";
import EditIcon from "@material-ui/icons/Edit";
import EmailIcon from "@material-ui/icons/Email";
import EmojiEventsIcon from "@material-ui/icons/EmojiEvents";
import Flag from "react-world-flags";
import Grid from "@material-ui/core/Grid";
import HomeWorkIcon from "@material-ui/icons/HomeWork";
import Link from "@material-ui/core/Link";
import Markdown from "../../../common/Markdown";
import NotesIcon from "@material-ui/icons/Notes";
import PersonIcon from "@material-ui/icons/Person";
import PhoneIcon from "@material-ui/icons/Phone";
import PublicIcon from "@material-ui/icons/Public";
import SchoolIcon from "@material-ui/icons/School";
import VerifiedUserIcon from "@material-ui/icons/VerifiedUser";
import countries from "country-list";

type ProfileHeaderProps = {
	className?: string;
	userID: number;
};
/* Styles */
const useStyles = makeStyles((theme: Theme) => ({
	avatar: {
		width: "200px",
		height: "200px",
		marginBottom: "25px",
		fontSize: "72px",
		color: theme.palette.getContrastText("#F39327"),
		backgroundColor: "#F39327",
	},
	avatarNameFlagBox: {
		alignItems: "center",
		display: "flex",
		flexDirection: "column",
		justifyContent: "center",
	},
	bioBox: {
		display: "flex",
		flexDirection: "row",
		marginTop: "-20px",
		marginLeft: "15px",
		justifyContent: "left",
		alignItems: "left",
	},
	edit: {
		display: "flex",
		justifyContent: "flex-end",
	},
	flag: {
		width: "50px",
		height: "50px",
		marginLeft: "25px",
		marginTop: "5px",
	},
	gridCenter: {
		width: "300px",
		marginTop: "35px",
		marginLeft: "25px",
		alignItems: "center",
		display: "flex",
		flexDirection: "column",
		justifyContent: "top",
	},
	infoBox: {
		display: "flex",
		flexDirection: "row",
		marginBottom: "15px",
	},
	infoIcon: {
		width: "30px",
		height: "30px",
		marginRight: "10px",
	},
	loading: {
		marginTop: "20px",
		marginBottom: "20px",
	},
	nameFlagBox: {
		display: "flex",
		flexDirection: "row",
		marginBottom: "20px",
	},
	nameText: {
		fontWeight: "bold",
	},
	profileInfoCard: {
		backgroundColor: "#EEEEEE",
		padding: "25px",
		borderRadius: "25px",
		marginLeft: "75px",
		width: "100%",
	},
	upvoteCountText: {
		marginRight: "10px",
	},
}));

export default function ProfileHeader(props: ProfileHeaderProps) {
	const classes = useStyles();
	const dispatch = useDispatch();
	const myUserID = useSelector(selectUserID);
	const [isError, setIsError] = useState(false); // Show error screen if fetching post fails.
	const [isFetching, setIsFetching] = useState(true); // Show loading screen if fetching.
	const [user, setUser] = useState<ProfileState>();

	// Overwrite some country code to make the flag show up properly
	countries.overwrite([
		{ code: "US", name: "United States" },
		{ code: "GB", name: "United Kingdom" },
	]);

	// User should only be able to edit their own profile
	const isEditable = isNaN(props.userID) || myUserID === props.userID;

	useEffect(() => {
		var user_id: number;
		if (!isNaN(props.userID)) {
			user_id = props.userID;
		} else {
			user_id = myUserID;
		}
		fetchProfile(user_id, (e) => dispatch(catchHandler(e))).then(
			(result) => {
				console.log("ProfileHeader", result);
				setIsFetching(false);
				if (result !== undefined) {
					setIsFetching(false);
					setUser(result);
				} else {
					// API could not fetch post based on provided user ID
					console.error("Unable to fetch user");
					setIsError(true);
				}
			}
		);
	}, [myUserID, props.userID, dispatch]);

	console.log(user);
	if (isFetching) {
		return <CircularProgress className={classes.loading} />;
	} else if (isError || user === undefined) {
		return (
			<Alert severity="error">
				The user you are looking for is currently unavailable. Please
				try again.
			</Alert>
		);
	} else {
		return (
			<Grid className={classes.gridCenter} item xs={4}>
				<Card variant="outlined" className={classes.profileInfoCard}>
					{isEditable && (
						<div className={classes.edit}>
							<Link href="/EditProfile" color="inherit">
								<EditIcon />
							</Link>
						</div>
					)}
					<CardContent>
						<Box className={classes.avatarNameFlagBox}>
							{/* User's avatar set to a default image for now*/}
							<AvatarIcon
								isSmall={false}
								initials={getInitials(user)}
							/>
							{/* Horizontal box with two elements - name and flag */}
							<Box
								className={classes.nameFlagBox}
								marginBottom={100}
							>
								{/* Name */}
								<Typography
									className={classes.nameText}
									variant="h3"
								>
									{user.firstName} {user.lastName}
								</Typography>
								{user.isAdmin && <VerifiedUserIcon />}
								{user.isCountryPublic && (
									<Flag
										code={countries.getCode(user.country)}
										className={classes.flag}
									/>
								)}
							</Box>
						</Box>
						<Box className={classes.infoBox}>
							<EmojiEventsIcon
								className={classes.infoIcon}
							></EmojiEventsIcon>
							<Typography
								variant="h5"
								className={classes.upvoteCountText}
							>
								Upvote Count:
							</Typography>
							<Typography variant="h5">
								{user.totalUpvotes > 0 ? user.totalUpvotes : 0}
							</Typography>
						</Box>
						<Box className={classes.infoBox}>
							<PersonIcon
								className={classes.infoIcon}
							></PersonIcon>
							<Typography variant="h5">
								{user.username}
							</Typography>
						</Box>
						{user.isResidentialCollegePublic ? (
							<Box className={classes.infoBox}>
								<HomeWorkIcon
									className={classes.infoIcon}
								></HomeWorkIcon>
								<Typography variant="h5">
									{user.college}
								</Typography>
							</Box>
						) : (
							false
						)}
						{user.isEmailPublic ? (
							<Box className={classes.infoBox}>
								<EmailIcon
									className={classes.infoIcon}
								></EmailIcon>
								<Typography variant="h5">
									{user.email}
								</Typography>
							</Box>
						) : (
							false
						)}
						{user.isPhonePublic ? (
							<Box className={classes.infoBox}>
								<PhoneIcon
									className={classes.infoIcon}
								></PhoneIcon>
								<Typography variant="h5">
									{user.phone}
								</Typography>
							</Box>
						) : (
							false
						)}
						{user.isCountryPublic ? (
							<Box className={classes.infoBox}>
								<PublicIcon
									className={classes.infoIcon}
								></PublicIcon>
								<Typography variant="h5">
									{user.country}
								</Typography>
							</Box>
						) : (
							false
						)}
						{user.isYearPublic ? (
							<Box className={classes.infoBox}>
								<SchoolIcon
									className={classes.infoIcon}
								></SchoolIcon>
								<Typography variant="h5">
									{user.year}
								</Typography>
							</Box>
						) : (
							false
						)}
						{user.bio === "" ? (
							false
						) : (
							<Box className={classes.infoBox}>
								<NotesIcon
									className={classes.infoIcon}
								></NotesIcon>
								<Typography variant="h5">
									{"Biography"}
								</Typography>
							</Box>
						)}
						{user.bio === "" ? (
							false
						) : (
							<Container className={classes.bioBox}>
								<Markdown md={user.bio} />
							</Container>
						)}
					</CardContent>
				</Card>
			</Grid>
		);
	}
}
