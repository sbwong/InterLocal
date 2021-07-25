import React, { ReactNode } from "react";
import ReduxProvider, {
	ReduxProviderProps
} from "../../../common/ReduxProvider";

import ReactDOM from "react-dom";
import { Signup } from "../Signup";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<Signup />
		</ReduxProvider>,
		div
	);
});

it("renders signup page", () => {
	const { signup } = render(
		<ReduxProvider>
			<Signup />
		</ReduxProvider>
	);
	expect(signup).toMatchSnapshot();
});
