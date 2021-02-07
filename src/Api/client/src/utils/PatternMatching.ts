export type Map<T, TDiscriminant extends keyof T> = T[TDiscriminant] extends string
  ? {
      [K in T[TDiscriminant]]: T extends { [P in TDiscriminant]: K } ? T : never;
    }
  : never;

export type Pattern<TInput, TDiscriminant extends keyof TInput, TOutput> = {
  [K in keyof Map<TInput, TDiscriminant>]: (value: Map<TInput, TDiscriminant>[K]) => TOutput;
};

export const match = <TInput, TDiscriminant extends keyof TInput, TOutput>(
  input: TInput,
  discriminant: TDiscriminant,
  pattern: Pattern<TInput, TDiscriminant, TOutput>,
): TOutput => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return pattern[input[discriminant]](input as any);
};
