import "./App.css";

import { persistor, store } from "./app/store";

import CookieConsentBar from "./common/CookieConsentBar";
import Footer from "./common/Footer";
import NavBar from "./common/NavBar";
import { PersistGate } from "redux-persist/integration/react";
import { Provider } from "react-redux";
import React from "react";
import Routes from "./constants/Routes";
import { ThemeProvider } from "@material-ui/styles";
import defaultTheme from "./Theme";

function App() {
    // const classes = useStyles();
    return (
        <ThemeProvider theme={defaultTheme}>
            <Provider store={store}>
                <PersistGate loading={null} persistor={persistor}>
                    <div className="App">
                        <NavBar />
                        <Routes />
                        <CookieConsentBar />
                        <Footer />
                    </div>
                </PersistGate>
            </Provider>
        </ThemeProvider>
    );
}

export default App;
