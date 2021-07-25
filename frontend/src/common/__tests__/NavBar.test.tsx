import React, { ReactNode } from "react";
import ReduxProvider, { ReduxProviderProps } from "../ReduxProvider";

import NavBar from "../NavBar";
import ReactDOM from "react-dom";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<NavBar />
		</ReduxProvider>,
		div
	);
});

it("renders nav bar", () => {
	const { navBar } = render(
		<ReduxProvider>
			<NavBar />
		</ReduxProvider>
	);
	expect(navBar).toMatchSnapshot();
});

it("renders sign up and login buttons when logged out", async () => {
	const { findAllByText } = render(
		<ReduxProvider>
			<NavBar />
		</ReduxProvider>
	);

	const signup = await findAllByText("Sign Up");
	const login = await findAllByText("Login");
	expect(signup).toBeTruthy();
	expect(login).toBeTruthy();
});
