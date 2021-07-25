import React from "react";
import ReactDOM from "react-dom";
import ReduxProvider from "../../../../common/ReduxProvider";
import VisitProfile from "../VisitProfile";
import { render } from "@testing-library/react";

it("renders without crashing", () => {
	const div = document.createElement("div");

	ReactDOM.render(
		<ReduxProvider>
			<VisitProfile username={"rochouhan6"} user_id={131} />
		</ReduxProvider>,
		div
	);
});

it("renders VisitProfile page", () => {
	const { visitprofile } = render(
		<ReduxProvider>
			<VisitProfile username={"rochouhan6"} user_id={131} />
		</ReduxProvider>
	);
	expect(visitprofile).toMatchSnapshot();
});
