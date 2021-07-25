import React, { ReactNode } from "react";
import ReduxProvider, {
	ReduxProviderProps
} from "../../../../common/ReduxProvider";

import ReactDOM from "react-dom";
import SignupBox from "../SignupBox";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<SignupBox />
		</ReduxProvider>,
		div
	);
});

it("renders edit profile header correctly", () => {
	const { signup } = render(
		<ReduxProvider>
			<SignupBox />
		</ReduxProvider>
	);
	expect(signup).toMatchSnapshot();
});
