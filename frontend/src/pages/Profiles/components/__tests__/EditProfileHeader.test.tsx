import React, { ReactNode } from "react";
import ReduxProvider, {
	ReduxProviderProps
} from "../../../../common/ReduxProvider";

import EditProfileHeader from "../EditProfileHeader";
import ReactDOM from "react-dom";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<EditProfileHeader />
		</ReduxProvider>,
		div
	);
});

it("renders edit profile header correctly", () => {
	const { signup } = render(
		<ReduxProvider>
			<EditProfileHeader />
		</ReduxProvider>
	);
	expect(signup).toMatchSnapshot();
});
