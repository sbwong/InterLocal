const PREFIX = "WebClient";
const metricName = (name: string) => `${PREFIX} ${name}`;

export const POST_CLICKED = metricName("PostClicked");
