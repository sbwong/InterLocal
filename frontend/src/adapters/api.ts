import { API_URL } from "../constants/Api";
import Axios from "axios";

const defaultOptions: Object = {
	headers: {
		"Content-Type": "application/json",
	},
	withCredentials: true,
};

export const makeAPIPostRequest = async (
	route: string,
	body: Object,
	version: string = "",
	options: Object = defaultOptions
) => {
	return await Axios.post(API_URL + version + route, body, options);
};

export const makeAPIGetRequest = async (
	route: string,
	options: Object = defaultOptions
) => {
	return await Axios.get(API_URL + route, options);
};

export const makeAPIPutRequest = async (
	route: string,
	body: Object,
	options: Object = defaultOptions
) => {
	return await Axios.put(API_URL + route, body, options);
};

export const makeAPIDeleteRequest = async (
	route: string, 
	body: Object,
	options: Object = defaultOptions
) => {
	return await Axios.delete(API_URL + route, Object.assign(body, defaultOptions));
};
