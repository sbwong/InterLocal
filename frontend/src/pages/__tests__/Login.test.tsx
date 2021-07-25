import React, { ReactNode } from "react";
import ReduxProvider, { ReduxProviderProps } from "../../common/ReduxProvider";

import Login from "../Profiles/Login";
import ReactDOM from "react-dom";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<Login />
		</ReduxProvider>,
		div
	);
});

it("renders login", () => {
	const { post } = render(
		<ReduxProvider>
			<Login />
		</ReduxProvider>
	);
	expect(post).toMatchSnapshot();
});
