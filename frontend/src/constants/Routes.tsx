import React, { FC } from "react";
import { Redirect, Route, RouteProps, Switch } from "react-router";
import { selectIsAuth, selectIsLoaded } from "../slices/profileSlice";

import { BrowserRouter } from "react-router-dom";
import CreatePost from "../pages/Posts/CreatePost";
import { EditProfile } from "../pages/Profiles/EditProfile";
import HelloComponent from "../pages/Hello";
import HomePage from "../pages/HomePage";
import Login from "../pages/Profiles/Login";
import { PrivacyPage } from "../pages/PrivacyPage";
import { Profile } from "../pages/Profiles/Profile";
import SearchResultsPage from "../pages/SearchResultsPage";
import { Signup } from "../pages/Profiles/Signup";
import TagFeed from "../pages/TagFeed";
import { UpdatePost } from "../pages/Posts/EditPost";
import { ViewPost } from "../pages/Posts/ViewPost";
import { useSelector } from "react-redux";

export interface AuthRouteProps extends RouteProps {
    isAuth: boolean;
}

export interface AuthLoadedRouteProps extends RouteProps, AuthRouteProps {
    isAuth: boolean;
    isLoaded: boolean;
}

/**
 * Defines a private route - if the user is NOT logged in or has an invalid token,
 * then we redirect them to the login page.
 */
const PrivateRoute: FC<AuthRouteProps> = ({ component, isAuth, ...rest }) => {
    // Check store for authentication
    // Everything looks good! Now let's send the user on their way
    return isAuth ? (
        <Route {...rest} component={component} />
    ) : (
        <Redirect to="/Login" />
    );
};

/**
 * Defines a public only route - if the user is logged in,
 * then we redirect them to the login page.
 */
const PublicOnlyRoute: FC<AuthLoadedRouteProps> = ({
    component,
    isAuth,
    isLoaded,
    ...rest
}) => {
    // Check store for authentication
    // Everything looks good! Now let's send the user on their way
    return isAuth ? (
        <Redirect to="/Home" />
    ) : (
        <Route {...rest} component={component} />
    );
};

/**
 * Defines all components and their paths
 */
export const routesArray = [
    {
        path: "/Hello",
        component: HelloComponent,
        publicOnly: true,
    },
    {
        path: "/SearchResults",
        component: SearchResultsPage,
        privateRoute: false,
    },
    {
        path: "/Profile/:user_id",
        component: Profile,
        privateRoute: true,
    },
    {
        path: "/Profile",
        component: Profile,
        privateRoute: true,
    },
    {
        path: "/EditProfile",
        component: EditProfile,
        privateRoute: true,
    },
    {
        path: "/Signup",
        component: Signup,
        publicOnly: true,
    },
    {
        path: "/Login",
        component: Login,
        publicOnly: true,
    },
    {
        path: "/Tag/:tag",
        component: TagFeed,
        privateRoute: false,
    },
    {
        path: "/Post/:post_id",
        component: ViewPost,
        privateRoute: false,
    },
    {
        path: "/EditPost/:post_id",
        component: UpdatePost,
        privateRoute: true,
    },
    {
        path: "/Home",
        component: HomePage,
        privateRoute: false,
    },
    {
        path: "/CreatePost",
        component: CreatePost,
        privateRoute: true,
    },
    {
        path: "/Privacy",
        component: PrivacyPage,
        privateRoute: false,
    },
    {
        path: "/",
        component: HomePage,
        privateRoute: false,
    },
];

/**
 * Defines all the routes for our system.
 * @param {*} param0
 */
export const Routes = () => {
    const isAuth = useSelector(selectIsAuth);
    const isLoaded = useSelector(selectIsLoaded);
    return (
        <BrowserRouter>
            <Switch key="switch">
                {routesArray.map((routeObject) => {
                    let {
                        path,
                        component,
                        privateRoute,
                        publicOnly,
                    } = routeObject;
                    if (privateRoute) {
                        return (
                            <PrivateRoute
                                path={path}
                                key={path}
                                component={component}
                                isAuth={isAuth}
                            />
                        );
                    } else if (publicOnly) {
                        return (
                            <PublicOnlyRoute
                                path={path}
                                key={path}
                                component={component}
                                isAuth={isAuth}
                                isLoaded={isLoaded}
                            />
                        );
                    } else {
                        return (
                            <Route
                                path={path}
                                key={path}
                                component={component}
                            />
                        );
                    }
                })}
            </Switch>
        </BrowserRouter>
    );
};

export default Routes;
