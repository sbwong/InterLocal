import Axios from "axios";
import { LOGGING_URL } from "../constants/Api";

const defaultOptions: Object = {
	headers: {
		"Content-Type": "application/json",
	},
};

export const logCustomMetric = async (
	key: string,
	value: number,
	message: string = "",
	options: Object = defaultOptions
) => {
	const requestBody = {
		"log_type": "CustomMetric",
		key,
		value,
		message,
	}
	return await Axios.post(LOGGING_URL, requestBody, options)
};

export const logIncrementCustomMetric = async (
	key: string,
	message: string = "",
	options: Object = defaultOptions
) => {
	return await logCustomMetric(key, 1, message, options);
}
