# Changelog

> ### WARNING
>
> (!) - means "possible subject to change"

## 0.2.1
### Changed
- Added generated materialized query page masks for bake-query systems.
- Inlined generated query add/remove/update paths and reused computed page ids and slot bits.
- Reworked archetype migration around an on-demand transition graph and transition LRU cache.
- Added opt-in inline component pages and fixed-size page array pooling.
- Fixed bitset equality and matching to compare full masks instead of relying on hash-only checks.
- Removed the old generic storage layer in favor of generated page storage.


## 0.1.6
### Changed
- Completely rewritten core rewritten to hand inlined logic instead of using a lot of wrappers and various collections
- World Api `Entities` call changed to `Iterate`
- A lot of performance compared to previous version
- this Changelog =)

## Added
- `Iterate` overloads for all cases up to 4 component types
- (!) `DeltaAttribute` - can be used to specify passed `delta` parameters (e.g. `Tick`'s deltaTime of something like that)
- (!) `UniformAttribute` - to allow pass uniforms into system methods
- Some basic tests
- Safety checks on api calls

## Removed
- All inner special collections
- Unused public types
- Temporary removed Unity integrations (need to be rewritten)

## 0.1.5 and previous

### Had
- Sparse Set based storage
- Group based system
- Some weird hacks and tricks
- Source generation for systems
