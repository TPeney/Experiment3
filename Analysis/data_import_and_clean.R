require(tidyverse)

files <- list.files(
  path = "./data_raw/behavioural_data",
  pattern = "trial_results.csv",
  full.names = TRUE,
  recursive = TRUE
)

data.raw <- bind_rows(lapply(files, read_csv,
  col_names = TRUE,
  # Quiet a warning related to auto col_type detection
  show_col_types = FALSE,
  # Explicitly specify the data types of certain cols
  cols(ppid = col_integer(), condition = col_character())
))

data.cleaned <- data.raw |>
  # Remove practice trials
  filter(blockType != "practice") |>
  # Remove redundant columns
  select(experiment, ppid, session_num, trial_num_in_block, RT, trialType, arraySize, targetID, trialPassed, condition) |>
  # Transform from sec to ms
  mutate(RT = RT * 1000) |>
  # Change accuracy from bool to int
  mutate(accuracy = if_else(trialPassed, 1, 0),
         .keep = "unused") |>
  # Sets an 'intrinsic order' for the conditions
  mutate(condition = factor(condition, levels = c("2D", "VRH", "VRG"))) |>
  # Remove erroneous data
  filter(RT > 0)

saveRDS(data.cleaned, file = "./data_processed/aggregated_behavioural_data") 
