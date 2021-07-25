import { createMuiTheme } from '@material-ui/core/styles';

const montserrat = [
    "Montserrat",
    "sans-serif"
].join(",")

const roboto = [
    "Roboto",
    "sans-serif"
].join(",")

const defaultTheme = createMuiTheme({
    palette: {
        type: "light",
        primary: {
            main: "#13133E",
            contrastText: "#FFFFFF"
        },
        secondary: {
            main: "#5BC0BE",
            contrastText: "#FFFFFF"
        },
        background: {
            default: "#F7F7F7"
        }
    },
    typography: {
        fontFamily: montserrat,
        h1: {
            fontFamily: montserrat,
            fontWeight: 900,
        },
        h2: {
            fontFamily: roboto,
            fontWeight: 900,
        },
        h3: {
            fontFamily: roboto,
            fontWeight: 900,
        },
        h4: {
            fontFamily: roboto,
            fontWeight: 900,
        },
        h5: {
            fontFamily: roboto,
            fontWeight: 900,
        },
        h6: {
            fontFamily: montserrat,
            fontWeight: 900,
        },
        subtitle1: {
            fontFamily: roboto,
        },
        subtitle2: {
            fontFamily: roboto,
        },
        body1: {
            fontFamily: roboto,
        },
        body2: {
            fontFamily: roboto,
        },
        button: {
            fontFamily: montserrat,
        },
        caption: {
            fontFamily: roboto,
        },
        overline: {
            fontFamily: roboto,
        }
    }
});

export default defaultTheme;