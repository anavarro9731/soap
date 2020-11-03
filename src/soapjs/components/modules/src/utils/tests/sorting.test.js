import { sortArrayByObjectPropertyAlphanumerically } from '../formatting';

describe('when calling sortArrayByObjectPropertyAlphanumerically', () => {
  let array, sortedArray, expectedArray;

  beforeEach(function arrange() {
    array = [
      { name: 'b' },
      { name: '1' },
      { name: 'c2' },
      { name: '34' },
      { name: '9c' },
    ];

    expectedArray = [
      { name: '1' },
      { name: '9c' },
      { name: '34' },
      { name: 'b' },
      { name: 'c2' },
    ];
  });

  beforeEach(function act() {
    sortedArray = sortArrayByObjectPropertyAlphanumerically(
      array,
      array => array.name,
    );
  });

  it('should correctly order the array alphanumerically', () => {
    expect(sortedArray).toEqual(expectedArray);
  });
});
