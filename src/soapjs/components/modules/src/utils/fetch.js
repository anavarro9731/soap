export const fetch = (endpoint, callback) => {
  try {
    global
      .fetch(endpoint)
      .then(response => response.json())
      .then(result => callback(result));
  } catch (e) {
    console.log(e);
  }
};
