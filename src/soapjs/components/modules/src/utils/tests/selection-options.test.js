import { timeSelectionOptions } from '../selection-options';

describe('when calling timeSelectionOptions with 15 minute intervals', () => {
  let interval, times;

  beforeEach(function arrange() {
    interval = 15;
  });

  beforeEach(function act() {
    times = timeSelectionOptions(interval);
  });

  it('should give the correct list of times', () => {
    expect(times.length).toEqual(96);

    expect(times[0]).toEqual({ value: '00:00', label: '00:00' });
    expect(times[1]).toEqual({ value: '00:15', label: '00:15' });
    expect(times[4]).toEqual({ value: '01:00', label: '01:00' });
    expect(times[95]).toEqual({ value: '23:45', label: '23:45' });
  });
});

describe('when calling timeSelectionOptions with 60 minute intervals', () => {
  let interval, times;

  beforeEach(function arrange() {
    interval = 60;
  });

  beforeEach(function act() {
    times = timeSelectionOptions(interval);
  });

  it('should give the correct list of times', () => {
    expect(times.length).toEqual(24);

    expect(times[0]).toEqual({ value: '00:00', label: '00:00' });
    expect(times[1]).toEqual({ value: '01:00', label: '01:00' });
    expect(times[4]).toEqual({ value: '04:00', label: '04:00' });
    expect(times[23]).toEqual({ value: '23:00', label: '23:00' });
  });
});

describe('when calling timeSelectionOptions with 13 minute intervals', () => {
  let interval;

  beforeEach(function arrange() {
    interval = 13;
  });

  it('should throw an error', () => {
    const x = () => timeSelectionOptions(interval);
    expect(x).toThrowError(
      /The 'interval' parameter must be a number that 60 is divisible by/,
    );
  });
});
