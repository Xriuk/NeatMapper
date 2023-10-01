# .NEaT Mapper

## What is this project

.NEaT Mapper is an object mapper, with configurable and reusable mappings.

This allows you to create mappings between different types (even generic ones),
combine them, nest them and reuse them, making your code DRY. Map once, use everywhere.

All of this is achieved with strongly typed maps, easily debuggable,
no compilation into obscure expressions, no black boxes.

It also supports async maps (coming soon) and services via Dependency Injection (DI).

## How to install

You can find all the packages on Nuget https://www.nuget.org/profiles/xriuk

Choose whether you need:
- The normal mapper ([NeatMapper](https://www.nuget.org/packages/NeatMapper))
- Or the async one (NeatMapper.Async) (coming soon)
- or both

If you are using Dependency Injection (DI) in your project you may want to install these packages instead:
- ([NeatMapper.DependencyInjection](https://www.nuget.org/packages/NeatMapper.DependencyInjection))
- (NeatMapper.Async.DependencyInjection) (coming soon)

They both include the relative package above

## How to use

Find specific instructions in the README of every package

- [NeatMapper](https://github.com/Xriuk/NeatMapper/blob/main/src/NeatMapper/README.md)