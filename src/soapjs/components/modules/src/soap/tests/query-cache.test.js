import queryCache from "../query-cache.js";
import { ApiQuery, ApiEvent } from "../messages.js";
import * as __ from '../util.js'

test("queries filtered results", () => {

  queryCache.addOrReplace(
    __.md5Hash(Object.assign(new ApiQuery(), { pointlessprop: "2342342342" })),
    Object.assign(new ApiEvent())
  );

  queryCache.addOrReplace(
    __.md5Hash(Object.assign(new ApiQuery(), { pointlessprop: "6788676576576" })),
    Object.assign(new ApiEvent())
  );

  queryCache.addOrReplace(
    __.md5Hash(Object.assign(new ApiQuery(), { pointlessprop: "57567464564" })),
    Object.assign(new ApiEvent())
  );

  let result = queryCache.query(Object.assign(new ApiQuery(), { pointlessprop: "2342342342" }), 10);

  expect(result).toBeInstanceOf(ApiEvent);
});

test("queries filtered results", () => {

  queryCache.addOrReplace(

    __.md5Hash(Object.assign(new ApiQuery(), { pointlessprop: "2342342342" })),
    Object.assign(new ApiEvent())
  );

  queryCache.addOrReplace(

    __.md5Hash(Object.assign(new ApiQuery(), { pointlessprop: "6788676576576" })),
    Object.assign(new ApiEvent())
  );

  queryCache.addOrReplace(

    __.md5Hash(Object.assign(new ApiQuery(), { pointlessprop: "57567464564" })),
    Object.assign(new ApiEvent())
  );

  let result = queryCache.query(Object.assign(new ApiQuery(), { pointlessprop: "6788676576576" }), 5);

  expect(result).toBeInstanceOf(ApiEvent);
});

