import React, { ReactNode } from "react";

import { Provider } from "react-redux";
import { store } from "../app/store";

export interface ReduxProviderProps {
	children: ReactNode;
}

export default function ReduxProvider(props: ReduxProviderProps) {
	return <Provider store={store}>{props.children}</Provider>;
}
