import { generateGuid } from '../guid';

const crypto = require('crypto');

Object.defineProperty(global.self, 'crypto', {
  value: {
    getRandomValues: arr => crypto.randomBytes(arr.length),
  },
});

describe('when calling generateGuid', () => {
  let generatedGuid, regexTest;

  beforeEach(function arrange() {
    regexTest = RegExp(
      '[\\da-fA-F]{8}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{4}-[\\da-fA-F]{12}',
    );
  });

  beforeEach(function act() {
    generatedGuid = generateGuid();
  });

  it('should generate a random GUID in the correct format', () => {
    expect(regexTest.test(generatedGuid)).toEqual(true);
  });
});
