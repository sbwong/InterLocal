import React, { useState } from "react";
import {
	Theme,
	createStyles,
	fade,
	makeStyles,
} from "@material-ui/core/styles";
import { selectInitials, selectIsAuth } from "../slices/profileSlice";
import { useDispatch, useSelector } from "react-redux";

import AddIcon from "@material-ui/icons/Add";
import AppBar from "@material-ui/core/AppBar";
import AvatarIcon from "./AvatarIcon";
import Backdrop from "@material-ui/core/Backdrop";
import Button from "@material-ui/core/Button";
import CircularProgress from "@material-ui/core/CircularProgress";
import IconButton from "@material-ui/core/IconButton";
import { ReactComponent as InterLocalLogo } from "../interlocal.svg";
import Link from "@material-ui/core/Link";
import Menu from "@material-ui/core/Menu";
import MenuItem from "@material-ui/core/MenuItem";
import MoreIcon from "@material-ui/icons/MoreVert";
import Toolbar from "@material-ui/core/Toolbar";
import { Typography } from "@material-ui/core";
import { logout } from "../slices/profileSlice";

const useStyles = makeStyles((theme: Theme) =>
	createStyles({
		backdrop: {
			zIndex: theme.zIndex.drawer + 1,
			color: "#f7f7f7",
		},
		title: {
			display: "none",
			[theme.breakpoints.up("sm")]: {
				display: "block",
				color: "white",
			},
		},
		search: {
			position: "relative",
			borderRadius: theme.shape.borderRadius,
			backgroundColor: fade(theme.palette.common.white, 0.15),
			"&:hover": {
				backgroundColor: fade(theme.palette.common.white, 0.25),
			},
			marginRight: theme.spacing(5),
			marginLeft: 0,
			width: "100%",
			[theme.breakpoints.up("sm")]: {
				marginLeft: theme.spacing(3),
				width: "auto",
			},
			flexGrow: 100,
			textAlign: "left",
		},
		inputRoot: {
			color: "inherit",
		},
		inputInput: {
			padding: theme.spacing(1, 1, 1, 0),
			// vertical padding + font size from searchIcon
			paddingLeft: `calc(1em + ${theme.spacing(4)}px)`,
			transition: theme.transitions.create("width"),
			width: "100%",
			[theme.breakpoints.up("md")]: {
				width: "20ch",
			},
		},
		sectionDesktop: {
			display: "none",
			[theme.breakpoints.up("md")]: {
				display: "flex",
			},
		},
		sectionMobile: {
			display: "flex",
			[theme.breakpoints.up("md")]: {
				display: "none",
			},
		},
		button: {
			marginRight: theme.spacing(2),
		},
	})
);

export default function NavBar() {
	const classes = useStyles();
	const initials: string = useSelector(selectInitials);
	const isAuth = useSelector(selectIsAuth);
	const dispatch = useDispatch();
	const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
	const [
		mobileMoreAnchorEl,
		setMobileMoreAnchorEl,
	] = React.useState<null | HTMLElement>(null);
	const [isLoading, setIsLoading] = useState(false);
	const isMenuOpen = Boolean(anchorEl);
	const isMobileMenuOpen = Boolean(mobileMoreAnchorEl);

	const handleProfileMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
		setAnchorEl(event.currentTarget);
	};

	const handleMobileMenuClose = () => {
		setMobileMoreAnchorEl(null);
	};

	const handleMenuClose = () => {
		setAnchorEl(null);
		handleMobileMenuClose();
	};

	const handleMobileMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
		setMobileMoreAnchorEl(event.currentTarget);
	};

	const handleLogout = () => {
		// TODO: handle logout
		setIsLoading(true);
		dispatch(
			logout(() => {
				handleMenuClose();
				setIsLoading(false);
			})
		);
	};

	const menuId = "primary-search-account-menu";
	const renderMenu = (
		<Menu
			anchorEl={anchorEl}
			anchorOrigin={{ vertical: "top", horizontal: "right" }}
			id={menuId}
			keepMounted
			transformOrigin={{ vertical: "top", horizontal: "right" }}
			open={isMenuOpen}
			onClose={handleMenuClose}
		>
			<Link href="/Profile" color="inherit">
				<MenuItem onClick={handleMenuClose}>Profile</MenuItem>
			</Link>
			<MenuItem onClick={handleLogout}>Log out</MenuItem>
		</Menu>
	);

	const mobileMenuId = "primary-search-account-menu-mobile";
	const renderMobileMenu = (
		<Menu
			anchorEl={mobileMoreAnchorEl}
			anchorOrigin={{ vertical: "top", horizontal: "right" }}
			id={mobileMenuId}
			keepMounted
			transformOrigin={{ vertical: "top", horizontal: "right" }}
			open={isMobileMenuOpen}
			onClose={handleMobileMenuClose}
		>
			{isAuth ? (
				<div>
					<Link href="/Profile" color="inherit" underline={"none"}>
						<MenuItem onClick={handleMobileMenuClose}>
							<p>Profile</p>
						</MenuItem>
					</Link>
					<MenuItem onClick={handleLogout}>
						<p>Log out</p>
					</MenuItem>
				</div>
			) : (
				<div>
					<Link href="/SignUp" color="inherit" underline={"none"}>
						<MenuItem>
							<p>Sign Up</p>
						</MenuItem>
					</Link>
					<Link href="/Login" color="inherit" underline={"none"}>
						<MenuItem>
							<p>Log in</p>
						</MenuItem>
					</Link>
				</div>
			)}
		</Menu>
	);

	return (
		<div>
			<Backdrop className={classes.backdrop} open={isLoading}>
				<CircularProgress color="inherit" />
			</Backdrop>
			<AppBar position="static">
				<Toolbar>
					<Link href="/Home" underline={"none"}>
						<Typography variant="button">
							<InterLocalLogo height={30} />
						</Typography>
					</Link>
					<div className={classes.search}>


						{/* Search Engine ID (cx=...) from Chuck Wang*/}
						{/* see https://developers.google.com/custom-search/docs/tutorial/implementingsearchbox for tutorial*/}
						<script async src="https://cse.google.com/cse.js?cx=7b6030e4c521c8ab2"></script>
						<div className={"gcse-searchbox-only"} data-resultsUrl={"/SearchResults"}></div>

					</div>
					<Link href="/CreatePost" underline={"none"}>
						<Button
							className={classes.button}
							variant="contained"
							color="secondary"
							startIcon={<AddIcon />}
						>
							Create post
						</Button>
					</Link>
					<div className={classes.sectionDesktop}>
						{isAuth ? (
							<div>
								<IconButton
									edge="end"
									aria-label="account of current user"
									aria-controls={menuId}
									aria-haspopup="true"
									onClick={handleProfileMenuOpen}
									color="inherit"
								>
									<AvatarIcon
										isSmall={true}
										initials={initials}
									/>
								</IconButton>
							</div>
						) : (
							<div>
								<Link href="/Signup" underline={"none"}>
									<Button
										className={classes.button}
										variant="contained"
										color="secondary"
									>
										Sign Up
									</Button>
								</Link>
								<Link href="/Login" underline={"none"}>
									<Button
										className={classes.button}
										variant="contained"
										color="secondary"
									>
										Login
									</Button>{" "}
								</Link>
							</div>
						)}
					</div>
					<div className={classes.sectionMobile}>
						<IconButton
							aria-label="show more"
							aria-controls={mobileMenuId}
							aria-haspopup="true"
							onClick={handleMobileMenuOpen}
							color="inherit"
						>
							<MoreIcon />
						</IconButton>
					</div>
				</Toolbar>
			</AppBar>
			{renderMobileMenu}
			{renderMenu}
		</div>
	);
}
