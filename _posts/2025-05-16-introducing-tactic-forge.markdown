# Personalized Chess Training Built From Your Own Games

I've been working on a side project I'm finally ready to share: **TacticForge** [tacticforge.fun](https://tacticforge.fun), a chess training platform that turns your real games into a personalized tactics curriculum.

## The idea

Most chess improvement sites hand you a pile of random puzzles. That's fine — until you realize you keep losing the same way, in the same kinds of positions, against the same kinds of moves. The puzzles you grind through have no relationship to the mistakes you actually make at the board.

TacticForge flips that around. Connect your Lichess or Chess.com account, and it:

- Pulls your recent games and analyzes them with Stockfish
- Finds the critical moments where you went wrong
- Turns those positions into bite-sized puzzles, tagged by motif (pin, fork, deflection, mating net, and so on)
- Feeds them back to you with spaced repetition, so the patterns you keep missing show up until they finally click

## What's in it

A few of the features I'm proudest of:

- **Personalized puzzles from your games** — every blunder becomes a lesson
- **Weakness training that adapts** — it figures out which tactical themes you're shaky on and drills them until you're not
- **Game analysis & opening insights** — move-by-move review plus a heatmap of where your opening repertoire is leaking rating points
- **Daily Challenge, Puzzle Streak, and Puzzle Rush** — for the days you just want to play, not study
- **Curated puzzle packs** — themed sets when you want a guided path instead of a personalized one

## Tech stack

For the curious: React + TypeScript + Vite on the front end, Tailwind and shadcn/ui for styling, Supabase for auth and storage, and a Stockfish service running in the background for analysis.

## Try it

It's free to use — no credit card, no daily puzzle limits on the core features. Connect a chess account whenever you're ready, or just play with the demo puzzle on the landing page first.

If you've ever finished a game thinking *"I always lose like this"*, this is the tool I wish I'd had years ago. I'd love to hear what you think.
